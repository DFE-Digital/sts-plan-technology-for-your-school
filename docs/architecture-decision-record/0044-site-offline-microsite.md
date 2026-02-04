# 0044 – Removal of Site Offline Microsite

**Status**: accepted

## Context and Problem Statement

ADR 0043 introduced a dedicated “site offline microsite” to support maintenance windows requiring the main application to be unavailable.

Since its introduction, the team has gained operational experience running real migrations and maintenance windows using this approach. That experience has demonstrated that the microsite introduces disproportionate complexity, operational risk, and ongoing maintenance cost relative to the problem it was intended to solve.

The service now requires a simpler, safer, and more operationally aligned mechanism for restricting user access during maintenance windows, without introducing additional deployable applications or code paths.

## Decision Drivers

- Minimise operational risk during high-stakes deployment and migration activities

- Reduce the number of independently deployable artefacts required to operate the service

- Avoid solutions that require swapping entire applications in and out of production traffic

- Reduce ongoing maintenance burden, duplication, and rebranding overhead

- Ensure maintenance behaviour is easy to reason about under time pressure

- Prefer reversible, configuration-based approaches over architectural divergence

- Align with existing deployment, monitoring, and quality-gate tooling

## Observed Issues With the Microsite Approach

Operational use of the microsite revealed several concrete issues:

1. Production traffic replacement risk

   The microsite requires replacing the main application container entirely. Any failure in the microsite (build, startup, configuration, health checks) results in complete service outage with no fallback.

   This failure mode was realised during a live migration when the microsite container failed to start, leaving the service unavailable at a critical point.

2. Increased blast radius during maintenance

   Maintenance windows are already high-risk periods. Introducing a second application that must be built, deployed, validated, and later removed increases the number of failure points at precisely the wrong time.

3. Ongoing maintenance and rebranding cost

   The microsite is a second web application that must:
   - Track framework and dependency updates

   - Be rebranded alongside the main service (e.g. GOV.UK rebrand)

   - Remain security-patched and compliant

This cost is incurred despite the microsite being inactive for the vast majority of the service’s lifetime.

1. Code duplication and tooling friction

   The microsite lives in the same repository as the main application but is deployed independently. This has resulted in unavoidable code duplication that:
   - Triggers SonarQube duplication rules

   - Blocks unrelated changes

   - Requires artificial suppression or workarounds

   These issues are structural rather than accidental.

2. Mismatch between solution complexity and problem scope

   The original problem was temporary access restriction during migrations, not long-term service replacement. The microsite solves this by introducing a second full application, which is a materially larger solution than required.

3. Reduced clarity under operational pressure

   During incidents or migrations, engineers must reason about:
   - Which application is live

   - Which container is serving traffic

   - Which health checks apply

   - Which logs correspond to user-visible behaviour

This additional cognitive load is counterproductive during time-critical operations.

## Decision Outcome

The site offline microsite will be removed.

The service will instead use a feature-level maintenance mode within the main application, implemented via configuration and/or feature flags.

### Replacement Approach

The replacement solution will:

- Keep the main application deployed at all times
  Gate user access via configuration (e.g. maintenance mode flag)
- Allow allow-listing of specific users (e.g. developers) during maintenance
- Serve a maintenance page directly from the main application
- Avoid swapping production traffic between separate applications
- Be reversible instantly without redeployment where possible

This approach:

- Reduces deployment and runtime risk
- Eliminates duplicated code and tooling conflicts
- Keeps maintenance behaviour close to the code it protects
- Aligns with how the service is actually operated

## Consequences

### Positive

- Reduced operational risk during migrations
- Fewer deployable artefacts to manage
- No additional application to maintain or rebrand
- Elimination of SonarQube duplication issues
- Clearer mental model during incidents
- Faster rollback and recovery paths

### Negative

- Some HTTP-level purity (e.g. global 503 responses) may be harder to enforce uniformly

- Health and readiness behaviour must be carefully designed within the main app

These trade-offs are considered acceptable given the operational benefits.

## Superseded Decisions

This ADR supersedes ADR 0043. The microsite approach is no longer considered appropriate for the service’s operational context.
