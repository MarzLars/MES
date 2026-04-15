const state = {
    products: [],
    projects: [],
    workOrders: [],
    selectedWorkOrderId: null,
    activeModalId: null,
};

const elements = {
    apiStatus: document.getElementById('api-status'),
    apiStatusDot: document.getElementById('api-status-dot'),
    lastRefresh: document.getElementById('last-refresh'),
    refreshButton: document.getElementById('refresh-button'),
    seedButton: document.getElementById('seed-button'),
    clearOrderButton: document.getElementById('clear-order-button'),
    openProjectModalButton: document.getElementById('open-project-modal-button'),
    openWorkOrderModalButton: document.getElementById('open-work-order-modal-button'),
    openLookupModalButton: document.getElementById('open-lookup-modal-button'),
    createProjectForm: document.getElementById('create-project-form'),
    projectNameInput: document.getElementById('project-name-input'),
    createProjectMessage: document.getElementById('create-project-message'),
    createWorkOrderForm: document.getElementById('create-work-order-form'),
    projectSelect: document.getElementById('project-select'),
    addLineButton: document.getElementById('add-line-button'),
    workOrderLines: document.getElementById('work-order-lines'),
    createWorkOrderMessage: document.getElementById('create-work-order-message'),
    clearWorkOrderModalButton: document.getElementById('clear-work-order-modal-button'),
    lookupWorkOrderForm: document.getElementById('lookup-work-order-form'),
    workOrderIdInput: document.getElementById('work-order-id-input'),
    lookupWorkOrderMessage: document.getElementById('lookup-work-order-message'),
    workOrderDetails: document.getElementById('work-order-details'),
    workOrderPreview: document.getElementById('work-order-preview'),
    productsTableBody: document.getElementById('products-table-body'),
    productsEmpty: document.getElementById('products-empty'),
    projectsTableBody: document.getElementById('projects-table-body'),
    projectsEmpty: document.getElementById('projects-empty'),
    workOrdersTableBody: document.getElementById('work-orders-table-body'),
    workOrdersEmpty: document.getElementById('work-orders-empty'),
    ordersTabTableBody: document.getElementById('orders-tab-table-body'),
    ordersTabEmpty: document.getElementById('orders-tab-empty'),
    productsCount: document.getElementById('products-count'),
    projectsCount: document.getElementById('projects-count'),
    workOrdersCount: document.getElementById('work-orders-count'),
    modalOverlay: document.getElementById('modal-overlay'),
    lineTemplate: document.getElementById('work-order-line-template'),
    tabButtons: Array.from(document.querySelectorAll('[data-tab-button]')),
    tabPanels: Array.from(document.querySelectorAll('[data-tab-panel]')),
    closeModalButtons: Array.from(document.querySelectorAll('[data-close-modal]')),
};

const modalFocusTargets = {
    'project-modal': () => elements.projectNameInput,
    'work-order-modal': () => elements.projectSelect,
    'lookup-modal': () => elements.workOrderIdInput,
    'details-modal': () => document.querySelector('#details-modal .modal-close'),
};

const dateFormatter = new Intl.DateTimeFormat('en-GB', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'UTC',
});

const weightFormatter = new Intl.NumberFormat('en-GB', {
    minimumFractionDigits: 3,
    maximumFractionDigits: 3,
});

initialize().catch(error => {
    setStatus('Failed to initialize dashboard.', 'error');
    showMessage(elements.createProjectMessage, error.message ?? String(error), 'error');
});

function initialize() {
    elements.refreshButton.addEventListener('click', () => refreshDashboard());
    elements.seedButton.addEventListener('click', seedDemoData);
    elements.clearOrderButton.addEventListener('click', () => clearWorkOrderForm());
    elements.openProjectModalButton.addEventListener('click', () => openModal('project-modal'));
    elements.openWorkOrderModalButton.addEventListener('click', () => openModal('work-order-modal'));
    elements.openLookupModalButton.addEventListener('click', () => openModal('lookup-modal'));
    elements.clearWorkOrderModalButton.addEventListener('click', () => clearWorkOrderForm({ keepProjectSelection: true }));
    elements.createProjectForm.addEventListener('submit', createProject);
    elements.createWorkOrderForm.addEventListener('submit', createWorkOrder);
    elements.lookupWorkOrderForm.addEventListener('submit', lookupWorkOrder);
    elements.addLineButton.addEventListener('click', () => addWorkOrderLineRow());
    elements.modalOverlay.addEventListener('click', closeActiveModal);

    elements.closeModalButtons.forEach(button => {
        button.addEventListener('click', () => closeModal(button.dataset.closeModal));
    });

    elements.tabButtons.forEach(button => {
        button.addEventListener('click', () => switchTab(button.dataset.tabTarget));
    });

    [elements.workOrdersTableBody, elements.ordersTabTableBody].forEach(tableBody => {
        tableBody.addEventListener('click', async event => {
            const viewButton = event.target.closest('.js-view-work-order');
            if (!viewButton) return;

            const workOrderId = Number(viewButton.dataset.workOrderId);
            if (!Number.isInteger(workOrderId) || workOrderId <= 0) return;

            try {
                await loadWorkOrderDetails(workOrderId, { quiet: true, openPopup: true });
            }
            catch {
                // The detail popup already surfaces the error state.
            }
        });
    });

    elements.workOrderLines.addEventListener('click', event => {
        const removeButton = event.target.closest('.line-remove');
        if (!removeButton) return;

        const row = removeButton.closest('[data-line-row]');
        if (!row) return;

        const rows = elements.workOrderLines.querySelectorAll('[data-line-row]');
        if (rows.length === 1) {
            resetWorkOrderLineRow(row);
            return;
        }

        row.remove();
        ensureAtLeastOneLineRow();
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && state.activeModalId) {
            closeActiveModal();
        }
    });

    addWorkOrderLineRow();
    renderEmptyWorkOrderDetails();
    switchTab('overview-panel');
    return refreshDashboard();
}

async function refreshDashboard() {
    setStatus('Loading dashboard...', 'loading');

    try {
        const [products, projects, workOrders] = await Promise.all([
            apiGet('/products'),
            apiGet('/projects'),
            apiGet('/work-orders?limit=20'),
        ]);

        state.products = Array.isArray(products) ? products : [];
        state.projects = Array.isArray(projects) ? projects : [];
        state.workOrders = Array.isArray(workOrders) ? workOrders : [];

        renderProducts();
        renderProjects();
        renderWorkOrders();
        renderMetrics();
        populateProjectSelect();
        populateProductSelects();
        updateDashboardTimestamp();
        setStatus('Dashboard loaded.', 'ready');

        if (state.selectedWorkOrderId !== null) {
            try {
                await loadWorkOrderDetails(state.selectedWorkOrderId, { quiet: true, openPopup: false });
            }
            catch {
                // Keep the dashboard usable even if the selected order was removed.
            }
        }
    }
    catch (error) {
        setStatus('Dashboard refresh failed.', 'error');
        showMessage(elements.createWorkOrderMessage, error.message ?? String(error), 'error');
    }
}

async function seedDemoData() {
    setStatus('Seeding demo data...', 'loading');
    showMessage(elements.lookupWorkOrderMessage, '', 'info');

    try {
        const response = await fetch('/test/seed', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error(await readResponseMessage(response));
        }

        setStatus('Demo data seeded.', 'ready');
        await refreshDashboard();
    }
    catch (error) {
        setStatus('Seeding failed.', 'error');
        showMessage(elements.createProjectMessage, error.message ?? String(error), 'error');
    }
}

async function createProject(event) {
    event.preventDefault();

    const name = elements.projectNameInput.value.trim();
    if (!name) {
        showMessage(elements.createProjectMessage, 'Please enter a project name.', 'error');
        return;
    }

    try {
        const response = await apiPost('/projects', { name });
        const created = await response.json();

        showMessage(elements.createProjectMessage, `Created project #${created.id}.`, 'success');
        elements.projectNameInput.value = '';
        await refreshDashboard();

        if (created?.id) {
            elements.projectSelect.value = String(created.id);
        }
    }
    catch (error) {
        showMessage(elements.createProjectMessage, error.message ?? String(error), 'error');
    }
}

async function createWorkOrder(event) {
    event.preventDefault();

    let lines = [];

    try {
        lines = collectWorkOrderLines();
    }
    catch (error) {
        showMessage(elements.createWorkOrderMessage, error.message ?? String(error), 'error');
        return;
    }

    const projectId = Number(elements.projectSelect.value);
    if (!Number.isInteger(projectId) || projectId <= 0) {
        showMessage(elements.createWorkOrderMessage, 'Choose a project before creating a work order.', 'error');
        return;
    }

    if (lines.length === 0) {
        showMessage(elements.createWorkOrderMessage, 'Add at least one valid line.', 'error');
        return;
    }

    try {
        const response = await apiPost('/work-orders', {
            projectId,
            lines,
        });

        const created = await response.json();
        showMessage(elements.createWorkOrderMessage, `Created work order #${created.id}.`, 'success');
        await refreshDashboard();
        clearWorkOrderForm({ keepProjectSelection: true });
        closeModal('work-order-modal');
        await loadWorkOrderDetails(created.id, { quiet: true, openPopup: true });
    }
    catch (error) {
        showMessage(elements.createWorkOrderMessage, error.message ?? String(error), 'error');
    }
}

async function lookupWorkOrder(event) {
    event.preventDefault();

    const workOrderId = Number(elements.workOrderIdInput.value);
    if (!Number.isInteger(workOrderId) || workOrderId <= 0) {
        showMessage(elements.lookupWorkOrderMessage, 'Enter a work order ID greater than zero.', 'error');
        return;
    }

    try {
        await loadWorkOrderDetails(workOrderId, { quiet: false, openPopup: true });
    }
    catch {
        // The form message already explains the failure.
    }
}

async function loadWorkOrderDetails(workOrderId, options = {}) {
    const { quiet = false, openPopup = true } = options;

    try {
        const order = await apiGet(`/work-orders/${workOrderId}`);
        state.selectedWorkOrderId = order.workOrderId;
        renderWorkOrderDetails(order);
        renderWorkOrders();
        elements.workOrderIdInput.value = String(order.workOrderId);

        if (!quiet) {
            showMessage(elements.lookupWorkOrderMessage, `Loaded work order #${order.workOrderId}.`, 'success');
        }

        if (openPopup) {
            openModal('details-modal');
        }

        return order;
    }
    catch (error) {
        state.selectedWorkOrderId = null;
        renderEmptyWorkOrderDetails();
        renderWorkOrders();

        if (!quiet) {
            showMessage(elements.lookupWorkOrderMessage, error.message ?? String(error), 'error');
        }

        throw error;
    }
}

function renderProducts() {
    if (state.products.length === 0) {
        elements.productsTableBody.innerHTML = '';
        elements.productsEmpty.hidden = false;
        return;
    }

    elements.productsEmpty.hidden = true;
    elements.productsTableBody.innerHTML = state.products.map(product => `
        <tr>
            <td>${escapeHtml(product.id)}</td>
            <td>${escapeHtml(product.name)}</td>
            <td>${formatWeight(product.unitWeightKilograms)}</td>
            <td>${formatDate(product.createdDateTimeUtc)}</td>
        </tr>
    `).join('');
}

function renderProjects() {
    if (state.projects.length === 0) {
        elements.projectsTableBody.innerHTML = '';
        elements.projectsEmpty.hidden = false;
        return;
    }

    elements.projectsEmpty.hidden = true;
    elements.projectsTableBody.innerHTML = state.projects.map(project => `
        <tr>
            <td>${escapeHtml(project.id)}</td>
            <td>${escapeHtml(project.name)}</td>
            <td>${formatDate(project.createdDateTimeUtc)}</td>
        </tr>
    `).join('');
}

function renderWorkOrders() {
    const markup = state.workOrders.map(workOrder => {
        const isSelected = workOrder.workOrderId === state.selectedWorkOrderId;
        const selectedAttribute = isSelected ? ' data-selected="true"' : '';

        return `
            <tr${selectedAttribute}>
                <td>${escapeHtml(workOrder.workOrderId)}</td>
                <td>${escapeHtml(workOrder.projectName)}</td>
                <td>${escapeHtml(workOrder.lineCount)}</td>
                <td>${formatWeight(workOrder.totalWeightInKilograms)}</td>
                <td>${formatDate(workOrder.createdDateTimeUtc)}</td>
                <td>
                    <button type="button" class="button button-secondary js-view-work-order" data-work-order-id="${escapeHtml(workOrder.workOrderId)}">
                        View
                    </button>
                </td>
            </tr>
        `;
    }).join('');

    if (state.workOrders.length === 0) {
        elements.workOrdersTableBody.innerHTML = '';
        elements.ordersTabTableBody.innerHTML = '';
        elements.workOrdersEmpty.hidden = false;
        elements.ordersTabEmpty.hidden = false;
        return;
    }

    elements.workOrdersEmpty.hidden = true;
    elements.ordersTabEmpty.hidden = true;
    elements.workOrdersTableBody.innerHTML = markup;
    elements.ordersTabTableBody.innerHTML = markup;
}

function renderMetrics() {
    elements.productsCount.textContent = String(state.products.length);
    elements.projectsCount.textContent = String(state.projects.length);
    elements.workOrdersCount.textContent = String(state.workOrders.length);
}

function renderWorkOrderDetails(order) {
    const markup = buildWorkOrderDetailsMarkup(order);
    elements.workOrderDetails.innerHTML = markup;
    elements.workOrderPreview.innerHTML = markup;
}

function renderEmptyWorkOrderDetails() {
    const emptyDetails = '<p class="empty">Choose a recent work order or use the find popup to load one.</p>';
    elements.workOrderDetails.innerHTML = emptyDetails;
    elements.workOrderPreview.innerHTML = '<p class="empty">No work order loaded yet.</p>';
}

function buildWorkOrderDetailsMarkup(order) {
    const lineRows = order.lines.map(line => `
        <tr>
            <td>${escapeHtml(line.workOrderLineId)}</td>
            <td>${escapeHtml(line.productId)} - ${escapeHtml(line.productName)}</td>
            <td>${escapeHtml(line.quantity)}</td>
            <td>${formatWeight(line.unitWeightKilograms)}</td>
            <td>${formatWeight(line.totalLineWeightInKilograms)}</td>
        </tr>
    `).join('');

    return `
        <div class="details-header">
            <div>
                <p class="section-label">Work order #${escapeHtml(order.workOrderId)}</p>
                <h3>${escapeHtml(order.projectName)}</h3>
                <p class="panel-copy">Project #${escapeHtml(order.projectId)} | Created ${formatDate(order.createdDateTimeUtc)}</p>
            </div>

            <dl class="summary-grid">
                <div>
                    <dt>Lines</dt>
                    <dd>${escapeHtml(order.lines.length)}</dd>
                </div>
                <div>
                    <dt>Total weight</dt>
                    <dd>${formatWeight(order.totalWeightInKilograms)}</dd>
                </div>
            </dl>
        </div>

        <div class="table-wrap compact-table-wrap">
            <table class="data-table">
                <thead>
                    <tr>
                        <th>Line</th>
                        <th>Product</th>
                        <th>Qty</th>
                        <th>Unit weight</th>
                        <th>Line weight</th>
                    </tr>
                </thead>
                <tbody>${lineRows}</tbody>
            </table>
        </div>
    `;
}

function populateProjectSelect() {
    const currentValue = elements.projectSelect.value;
    const options = ['<option value="">Select a project</option>'];

    for (const project of state.projects) {
        const value = String(project.id);
        const selected = value === currentValue ? ' selected' : '';
        options.push(`<option value="${escapeHtml(value)}"${selected}>${escapeHtml(project.id)} - ${escapeHtml(project.name)}</option>`);
    }

    elements.projectSelect.innerHTML = options.join('');
    elements.projectSelect.disabled = state.projects.length === 0;

    if (state.projects.length === 0) {
        elements.projectSelect.value = '';
        return;
    }

    if (!elements.projectSelect.value) {
        elements.projectSelect.value = String(state.projects[0].id);
    }
}

function populateProductSelects() {
    const selects = elements.workOrderLines.querySelectorAll('.line-product');

    selects.forEach(select => {
        const currentValue = select.value;

        if (state.products.length === 0) {
            select.disabled = true;
            select.innerHTML = '<option value="">Seed demo data to load products</option>';
            select.value = '';
            return;
        }

        const options = ['<option value="">Select a product</option>'];
        for (const product of state.products) {
            const value = String(product.id);
            const selected = value === currentValue ? ' selected' : '';
            options.push(`<option value="${escapeHtml(value)}"${selected}>${escapeHtml(product.id)} - ${escapeHtml(product.name)}</option>`);
        }

        select.disabled = false;
        select.innerHTML = options.join('');

        if (!select.value) {
            select.value = String(state.products[0].id);
        }
    });
}

function addWorkOrderLineRow() {
    const row = elements.lineTemplate.content.firstElementChild.cloneNode(true);
    elements.workOrderLines.appendChild(row);
    populateProductSelects();
}

function resetWorkOrderLineRow(row) {
    const productSelect = row.querySelector('.line-product');
    const quantityInput = row.querySelector('.line-quantity');

    if (quantityInput) {
        quantityInput.value = '1';
    }

    populateProductSelects();

    if (productSelect && state.products.length > 0) {
        productSelect.value = String(state.products[0].id);
    }
}

function ensureAtLeastOneLineRow() {
    if (elements.workOrderLines.querySelector('[data-line-row]')) return;
    addWorkOrderLineRow();
}

function clearWorkOrderForm(options = {}) {
    const { keepProjectSelection = true } = options;
    const currentProject = keepProjectSelection ? elements.projectSelect.value : '';

    elements.workOrderLines.innerHTML = '';
    addWorkOrderLineRow();
    populateProjectSelect();

    if (currentProject) {
        elements.projectSelect.value = currentProject;
    }

    showMessage(elements.createWorkOrderMessage, '', 'info');
}

function collectWorkOrderLines() {
    const rows = Array.from(elements.workOrderLines.querySelectorAll('[data-line-row]'));
    const lines = [];

    rows.forEach((row, index) => {
        const productSelect = row.querySelector('.line-product');
        const quantityInput = row.querySelector('.line-quantity');
        const productId = Number(productSelect?.value);
        const quantity = Number(quantityInput?.value);

        if (!Number.isInteger(productId) || productId <= 0) {
            throw new Error(`Select a product for line ${index + 1}.`);
        }

        if (!Number.isInteger(quantity) || quantity <= 0) {
            throw new Error(`Enter a quantity greater than zero for line ${index + 1}.`);
        }

        lines.push({
            productId,
            quantity,
        });
    });

    return lines;
}

function switchTab(targetId) {
    elements.tabButtons.forEach(button => {
        const isActive = button.dataset.tabTarget === targetId;
        button.classList.toggle('is-active', isActive);
        button.setAttribute('aria-selected', String(isActive));
    });

    elements.tabPanels.forEach(panel => {
        const isActive = panel.id === targetId;
        panel.classList.toggle('is-active', isActive);
        panel.hidden = !isActive;
    });
}

function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    closeActiveModal({ restoreFocus: false });

    state.activeModalId = modalId;
    elements.modalOverlay.hidden = false;
    modal.hidden = false;
    document.body.classList.add('modal-open');

    const focusTarget = modalFocusTargets[modalId]?.();
    focusTarget?.focus?.();
}

function closeModal(modalId, options = {}) {
    const { restoreFocus = true } = options;
    const modal = document.getElementById(modalId);
    if (!modal) return;

    modal.hidden = true;

    if (state.activeModalId === modalId) {
        state.activeModalId = null;
        elements.modalOverlay.hidden = true;
        document.body.classList.remove('modal-open');
    }

    if (restoreFocus) {
        restoreModalTriggerFocus(modalId);
    }
}

function closeActiveModal(options = {}) {
    if (!state.activeModalId) return;
    closeModal(state.activeModalId, options);
}

function restoreModalTriggerFocus(modalId) {
    const triggerMap = {
        'project-modal': elements.openProjectModalButton,
        'work-order-modal': elements.openWorkOrderModalButton,
        'lookup-modal': elements.openLookupModalButton,
    };

    triggerMap[modalId]?.focus?.();
}

async function apiGet(path) {
    const response = await fetch(path, {
        headers: {
            'Accept': 'application/json',
        },
    });

    if (!response.ok) {
        throw new Error(await readResponseMessage(response));
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json();
}

async function apiPost(path, body) {
    const response = await fetch(path, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        },
        body: JSON.stringify(body),
    });

    if (!response.ok) {
        throw new Error(await readResponseMessage(response));
    }

    return response;
}

async function readResponseMessage(response) {
    const contentType = response.headers.get('content-type') ?? '';

    if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
        const body = await response.json();

        if (typeof body === 'string') {
            return body;
        }

        if (body?.title) {
            if (body.detail) {
                return `${body.title}: ${body.detail}`;
            }

            return body.title;
        }

        if (body?.errors && typeof body.errors === 'object') {
            const messages = Object.entries(body.errors)
                .flatMap(([field, values]) => values.map(value => `${field}: ${value}`));

            if (messages.length > 0) {
                return messages.join(' ');
            }
        }

        return JSON.stringify(body);
    }

    const text = await response.text();
    return text || `${response.status} ${response.statusText}`;
}

function updateDashboardTimestamp() {
    elements.lastRefresh.textContent = `Last refreshed ${dateFormatter.format(new Date())}.`;
}

function setStatus(message, stateName) {
    elements.apiStatus.textContent = message;
    elements.apiStatusDot.dataset.state = stateName;
}

function showMessage(element, message, tone) {
    element.textContent = message;
    element.dataset.tone = tone;
}

function formatDate(value) {
    if (!value) return '-';
    return dateFormatter.format(new Date(value));
}

function formatWeight(value) {
    if (value === null || value === undefined || Number.isNaN(Number(value))) return '-';
    return `${weightFormatter.format(Number(value))} kg`;
}

function escapeHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
