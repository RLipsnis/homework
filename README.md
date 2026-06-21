# Homework
Small, correct, and testable REST API for a simplified insurance domain

## Tech

- .NET 10 Web API (Controllers)
- In-memory storage (`ConcurrentDictionary`), no database
- OpenAPI document + Swagger UI
- xUnit + `WebApplicationFactory` integration tests

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)


## Run the API

cd SimpleInsuranceApp
dotnet run

Then open the Swagger UI:

- Swagger UI: http://localhost:5080/swagger

## Run the tests

cd SimpleInsuranceApp.Tests
dotnet test

**Policies**

- `EndDate` must be after `StartDate` → `400`.
- A policy can be activated only from `Draft` → otherwise `409`.
- A customer may not have two **active** policies of the **same product type** with
  **overlapping** date ranges. Activating such a policy returns `409`.
  Periods are treated as inclusive on both ends, so touching ranges count as overlapping.

**Claims**

- A claim can be created only for an **active** policy → otherwise `409`.
- `IncidentDate` must be within the policy period (inclusive) → otherwise `409`.
- A claim can be decided only from `New` → otherwise `409`.
- Rejecting a claim requires a `DecisionReason` → otherwise `400`.

## Error contract

All errors are returned as ProblemDetails (`application/problem+json`) with a consistent shape:

| Status | Meaning                                                              |
| ------ | ------------------------------------------------------------------- |
| `400`  | Invalid input / validation error                                    |
| `404`  | Resource not found (including a referenced customer or policy)      |
| `409`  | Business conflict (wrong status, overlap, incident outside period)  |

Domain failures are modelled as a small set of exceptions (`ValidationException`,
`NotFoundException`, `ConflictException`) and translated centrally by an
`IExceptionHandler`, so controllers stay free of error-handling branches.

## Design notes & tradeoffs


- **Date type — `DateOnly`.** Policy periods and incident dates are calendar dates with no
  meaningful time-of-day or time zone, so `DateOnly` models the domain precisely and
  avoids off-by-one bugs from time/UTC conversions; it serializes cleanly as `yyyy-MM-dd`.

- **Layering — controllers → services. Thin controllers do only HTTP; services
  own every business rule; an `InMemoryStore` singleton holds the data. This keeps rules
  testable in isolation and the HTTP layer trivial.

- **Error handling — exceptions mapped centrally.** Rules throw a small set of domain
  exceptions (`Validation`/`NotFound`/`Conflict`) that one `IExceptionHandler` turns into
  ProblemDetails. This keeps a single, consistent error contract and zero error branching
  in controllers. _Tradeoff:_ exceptions as control flow have a cost on hot paths; for a
  validation-heavy API at low volume the readability win is worth it. A result-type
  (`Result<T>`) approach is the main alternative.

- **Concurrency** The dictionaries are thread-safe per key, but the overlap
  check on activate and the status check on decide are read-then-write sequences, so they
  run under one shared lock to stay atomic. _Tradeoff:_ a single global lock serializes all
  activations/decisions — perfectly fine for an in-memory single instance, but it wouldn't
  scale out. A real system would push these invariants into the database (unique
  constraints / transactions).

## Improvements (next steps if continued)

Given more time, the highest-value additions, roughly in priority order:

1. **Persistence.** Replace `InMemoryStore` with EF Core (or another repository) behind the
   same seam, and enforce the overlap rule with a database constraint/transaction instead
   of the in-process lock.
3. **Richer validation surface.** Reject unknown JSON fields, cap string lengths and
   decimal precision (e.g. `Premium`/`AmountRequested` scale), and validate that incident
   dates aren't in the future where that matters.
4. **More test depth.** Property-based tests for the overlap rule (inclusive boundaries,
   adjacent vs. nested ranges) and a few pure unit tests on `Policy.OverlapsWith` alongside
   the integration tests.
5. **Observability & ops.** Structured logging with correlation ids, a `/health` endpoint,
   and a `traceId` surfaced in every ProblemDetails response.
6. **API hardening.** Optimistic concurrency (ETags) on the mutation endpoints, idempotency
   keys for `POST`s, and API versioning (`/v1`).
7. **CI.** A GitHub Actions workflow running `dotnet build` + `dotnet test` on every push.
