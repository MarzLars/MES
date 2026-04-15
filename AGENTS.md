# AGENTS.md for MES

## RULES

- Prefer immutable types
- Prefer Records over classes for immutable types
- Where suitable, prefer domain-appropriate standard exceptions for invalid input; keep invalid states unrepresentable in the model.
- Make invalid states unrepresentable. 
- C#14 syntax with rule of prefering the least verbose syntax.
- Apply CQRS pinciples
- C# Minimal APIs

# Record Design

- Define record's properties on the same line with the record declaration.
- Accompany each '<name>' with '<name>Factory' static factory class.
- Place the factory class in the same file as the record
- Expose static 'Create' method in the factory class instantiation.
- Place argument validation in the 'Create' method.
- Never use record's constructor when there is a factory method.
- Use immutable collections in records unless requested otherwise.
- Use 'ImmutableList<T>' in record whenever possible.
- Define record behaviour in extension methods in other static classes.

## Discriminated Unions

- Prefer using records for discriminated unions.
- Derive specific types from a base abstract record.
- Define the entire discriminated union in a single file.
- Define one static factories class per discriminated union.
- Expose one static factory method per variant.
- Follow all rules for record design when designing a discriminated union.