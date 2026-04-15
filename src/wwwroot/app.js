const state = {
    products: [],
    projects: [],
    workOrders: [],
    selectedWorkOrderId: null,
};

const elements = {
    apiStatus: document.getElementById('api-status'),
    apiStatusDot: document.getElementById('api-status-dot'),
    lastRefresh: document.getElementById('last-refresh'),
    refreshButton: document.getElementById('refresh-button'),
    seedButton: document.getElementById('seed-button'),
    clearOrderButton: document.getElementById('clear-order-button'),
    createProjectForm: document.getElementById('create-project-form'),
    projectNameInput: document.getElementById('project-name-input'),
    createProjectMessage: document.getElementById('create-project-message'),
    createWorkOrderForm: document.getElementById('create-work-order-form'),
    projectSelect: document.getElementById('project-select'),
    addLineButton: document.getElementById('add-line-button'),
    workOrderLines: document.getElementById('work-order-lines'),
    createWorkOrderMessage: document.getElementById('create-work-order-message'),
    lookupWorkOrderForm: document.getElementById('lookup-work-order-form'),
    workOrderIdInput: document.getElementById('work-order-id-input'),
    lookupWorkOrderMessage: document.getElementById('lookup-work-order-message'),
    workOrderDetails: document.getElementById('work-order-details'),
    productsTableBody: document.getElementById('products-table-body'),
    productsEmpty: document.getElementById('products-empty'),
    projectsTableBody: document.getElementById('projects-table-body'),
    projectsEmpty: document.getElementById('projects-empty'),
    workOrdersTableBody: document.getElementById('work-orders-table-body'),
    workOrdersEmpty: document.getElementById('work-orders-empty'),
    lineTemplate: document.getElementById('work-order-line-template'),
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
    elements.createProjectForm.addEventListener('submit', createProject);
    elements.createWorkOrderForm.addEventListener('submit', createWorkOrder);
    elements.lookupWorkOrderForm.addEventListener('submit', lookupWorkOrder);
    elements.addLineButton.addEventListener('click', () => addWorkOrderLineRow());

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

    addWorkOrderLineRow();
    return refreshDashboard();
}

async function refreshDashboard() {
    setStatus('Loading dashboard…', 'loading');

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
        populateProjectSelect();
        populateProductSelects();
        updateDashboardTimestamp();
        setStatus('Dashboard loaded.', 'ready');

        if (state.selectedWorkOrderId !== null) {
            try {
                await loadWorkOrderDetails(state.selectedWorkOrderId, { quiet: true });
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
    setStatus('Seeding demo data…', 'loading');
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

    const projectId = Number(elements.projectSelect.value);
    if (!Number.isInteger(projectId) || projectId <= 0) {
        showMessage(elements.createWorkOrderMessage, 'Choose a project before creating a work order.', 'error');
        return;
    }

    const lines = collectWorkOrderLines();
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
        await loadWorkOrderDetails(created.id);
        clearWorkOrderForm({ keepProjectSelection: true });
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

    await loadWorkOrderDetails(workOrderId);
}

async function loadWorkOrderDetails(workOrderId, options = {}) {
    const { quiet = false } = options;

    try {
        const order = await apiGet(`/work-orders/${workOrderId}`);
        state.selectedWorkOrderId = order.workOrderId;
        renderWorkOrderDetails(order);
        elements.workOrderIdInput.value = String(order.workOrderId);

        if (!quiet) {
            showMessage(elements.lookupWorkOrderMessage, `Loaded work order #${order.workOrderId}.`, 'success');
        }

        return order;
    }
    catch (error) {
        state.selectedWorkOrderId = null;
        renderEmptyWorkOrderDetails();

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
    if (state.workOrders.length === 0) {
        elements.workOrdersTableBody.innerHTML = '';
        elements.workOrdersEmpty.hidden = false;
        return;
    }

    elements.workOrdersEmpty.hidden = true;
    elements.workOrdersTableBody.innerHTML = state.workOrders.map(workOrder => `
        <tr>
            <td>${escapeHtml(workOrder.workOrderId)}</td>
            <td>${escapeHtml(workOrder.projectName)}</td>
            <td>${escapeHtml(workOrder.lineCount)}</td>
            <td>${formatWeight(workOrder.totalWeightInKilograms)}</td>
            <td>${formatDate(workOrder.createdDateTimeUtc)}</td>
            <td>
                <button type="button" class="button secondary small js-view-work-order" data-work-order-id="${escapeHtml(workOrder.workOrderId)}">
                    View
                </button>
            </td>
        </tr>
    `).join('');

    elements.workOrdersTableBody.querySelectorAll('.js-view-work-order').forEach(button => {
        button.addEventListener('click', async () => {
            const workOrderId = Number(button.dataset.workOrderId);
            if (!Number.isInteger(workOrderId) || workOrderId <= 0) return;

            try {
                await loadWorkOrderDetails(workOrderId);
            }
            catch {
                // The detail panel already shows the error.
            }
        });
    });
}

function renderWorkOrderDetails(order) {
    const lineRows = order.lines.map(line => `
        <tr>
            <td>${escapeHtml(line.workOrderLineId)}</td>
            <td>${escapeHtml(line.productId)} — ${escapeHtml(line.productName)}</td>
            <td>${escapeHtml(line.quantity)}</td>
            <td>${formatWeight(line.unitWeightKilograms)}</td>
            <td>${formatWeight(line.totalLineWeightInKilograms)}</td>
        </tr>
    `).join('');

    elements.workOrderDetails.innerHTML = `
        <div class="details-header">
            <div>
                <p class="eyebrow">Work order #${escapeHtml(order.workOrderId)}</p>
                <h3>${escapeHtml(order.projectName)}</h3>
                <p class="muted">Project #${escapeHtml(order.projectId)} · Created ${formatDate(order.createdDateTimeUtc)}</p>
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
            <table class="data-table details-table">
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

function renderEmptyWorkOrderDetails() {
    elements.workOrderDetails.innerHTML = '<p class="empty">Choose a work order from the list below or load one by ID.</p>';
}

function populateProjectSelect() {
    const currentValue = elements.projectSelect.value;
    const options = [];

    options.push('<option value="">Select a project</option>');
    for (const project of state.projects) {
        const value = String(project.id);
        const selected = value === currentValue ? ' selected' : '';
        options.push(`<option value="${escapeHtml(value)}"${selected}>${escapeHtml(project.id)} — ${escapeHtml(project.name)}</option>`);
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
        const options = [];

        if (state.products.length === 0) {
            options.push('<option value="">Seed demo data to load products</option>');
            select.disabled = true;
            select.innerHTML = options.join('');
            select.value = '';
            return;
        }

        options.push('<option value="">Select a product</option>');
        for (const product of state.products) {
            const value = String(product.id);
            const selected = value === currentValue ? ' selected' : '';
            options.push(`<option value="${escapeHtml(value)}"${selected}>${escapeHtml(product.id)} — ${escapeHtml(product.name)}</option>`);
        }

        select.innerHTML = options.join('');
        select.disabled = false;

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
    const quantityInput = row.querySelector('.line-quantity');
    if (quantityInput) {
        quantityInput.value = '1';
    }

    populateProductSelects();
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
    if (!value) return '—';
    return dateFormatter.format(new Date(value));
}

function formatWeight(value) {
    if (value === null || value === undefined || Number.isNaN(Number(value))) return '—';
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
