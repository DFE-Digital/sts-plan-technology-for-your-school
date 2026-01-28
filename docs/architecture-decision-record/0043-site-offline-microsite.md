# 0043 - Site Offline Microsite for Maintenance Windows

- **Status**: accepted

## Context and Problem Statement

The plan technology for your school service requires periodic maintenance windows for database migrations and infrastructure updates. During these windows, the service needs to be temporarily unavailable to users while displaying an informative maintenance page. The solution must:

- Display a user-friendly maintenance page that matches the service's design language
- Return appropriate HTTP status codes for monitoring and search engines
- Support health/readiness probes for infrastructure orchestration
- Integrate with the existing continuous deployment pipeline
- Work within current Azure Front Door configuration constraints

## Decision Drivers

- Azure Front Door configuration changes are currently disabled, preventing redirect-based solutions
- Database migrations require the main application to be offline
- Need to maintain infrastructure health checks (liveness/readiness probes)
- Deployment pipeline requires manual approval gates between critical steps
- Must provide clear user communication during maintenance
- Search engines and monitoring tools need appropriate HTTP status codes (503)
- Need to test maintenance page functionality independently
- Prefer solutions that utilize existing infrastructure over provisioning new resources (avoiding Terraform complexity)

## Considered Options

### Option 1: Static HTML Page Only

A single static HTML file deployed to blob storage or served directly.

**Pros:**

- Simple to implement
- Minimal infrastructure
- Fast to serve

**Cons:**

- Cannot set HTTP 503 status code (would return 200)
- No health check endpoint differentiation
- Cannot set Retry-After headers
- Limited testability
- Does not match existing design patterns
- Would require new infrastructure (blob storage/basic web app) necessitating Terraform changes and additional deployment complexity

### Option 2: Azure Front Door Redirect

Configure Azure Front Door to redirect all traffic to a maintenance page during downtime.

**Pros:**

- Centralized traffic management
- No application deployment needed

**Cons:**

- Azure Front Door configuration changes are currently disabled
- Would require infrastructure changes during critical maintenance window
- Risk of misconfiguration affecting production traffic

### Option 3: Dedicated Microsite Application (Selected)

A separate lightweight ASP.NET Core MVC application that serves the maintenance page with full control over HTTP responses.

**Pros:**

- Full control over HTTP status codes (503 for maintenance, 200 for health)
- Can implement proper health/readiness endpoints
- Supports Retry-After headers
- Matches main application's architecture (MVC pattern)
- Independently testable with WebApplicationFactory
- Uses GovUK Frontend for consistent styling
- Can be deployed before main application shutdown
- Reuses existing container infrastructure (no new Terraform resources required)

**Cons:**

- Additional application to maintain
- Requires container deployment infrastructure

## Decision Outcome

**Chosen option: "Option 3: Dedicated Microsite Application"**

We will implement a standalone ASP.NET Core MVC microsite that:

### Technical Implementation

1. **HTTP Status Codes**
   - Returns HTTP 503 (Service Unavailable) for all content pages
   - Returns HTTP 200 (OK) for `/health` endpoint to keep infrastructure happy
   - Includes `Retry-After: 3600` header (1 hour) on maintenance pages

2. **Health Check Design**
   - `/health` endpoint returns 200 OK with JSON indicating maintenance mode:

     ```json
     {
       "status": "maintenance",
       "healthy": true,
       "message": "Maintenance microsite is operational",
       "mainSiteAvailable": false,
       "timestamp": "2024-11-04T10:30:00Z"
     }
     ```

   - This keeps Azure Container Apps from removing the container / redirecting traffic away from what it percieves as an unhealthy container
   - Monitoring systems can parse the response to detect maintenance mode (i.e. intentional unavailability)

3. **Routing Architecture**
   - Catch-all route pattern `{**catchall}` directs all paths to maintenance controller
   - Supports all HTTP methods (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
   - Static assets (CSS, JS, images) served normally for page functionality

4. **Content & Styling**
   - Uses GovUK Frontend AspNetCore library for consistent design
   - Implements DfE header/footer matching main application
   - MVC pattern with layout pages for maintainability
   - Node.js build process for CSS/JS compilation (matching main app)

5. **HTTP Security Headers** ✅ Implemented
   - Removes server identification headers per [OWASP headers_remove.json](https://owasp.org/www-project-secure-headers/ci/headers_remove.json)
     - Includes ASP.NET specific headers (`Server`, `X-Powered-By`, `X-AspNet-Version`, `X-AspNetMvc-Version`)
     - Plus 30+ additional disclosure headers (framework identifiers, tracing IDs, etc.)
   - Adds protective security headers per [OWASP headers_add.json](https://owasp.org/www-project-secure-headers/ci/headers_add.json):
     - `X-Content-Type-Options: nosniff` - Prevents MIME type sniffing
     - `X-Frame-Options: deny` - Prevents clickjacking
     - `X-DNS-Prefetch-Control: off` - Disables DNS prefetching
     - `X-Permitted-Cross-Domain-Policies: none` - Restricts cross-domain policies
     - `Content-Security-Policy` - Restricts resource loading with upgrade-insecure-requests
     - `Cross-Origin-Embedder-Policy: require-corp` - Requires CORP for embedded resources
     - `Cross-Origin-Opener-Policy: same-origin` - Isolates browsing context
     - `Cross-Origin-Resource-Policy: same-origin` - Restricts resource loading
     - `Strict-Transport-Security: max-age=31536000; includeSubDomains` - Forces HTTPS
     - `Referrer-Policy: no-referrer` - Prevents referrer leakage
     - `Permissions-Policy` - Comprehensive feature policy disabling 25+ browser features
     - `Cache-Control: no-store, max-age=0` - Prevents caching (except /health endpoint)
   - 30 dedicated security header tests ensure proper implementation

### Deployment Pipeline Integration

The continuous deployment workflow has been modified to support maintenance windows:

1. **Pre-Migration Phase**
   - Scale up site offline microsite
   - Route traffic to microsite
   - Verify health checks passing
   - **Manual approval gate** via GitHub Actions environment

2. **Migration Phase**
   - Scale down main application
   - Execute automated database migrations
   - **Manual approval gate** for any manual migration steps

3. **Post-Migration Phase**
   - Scale up main application
   - Verify application health
   - Route traffic back to main application
   - **Manual approval gate** before scaling down microsite
   - Scale down site offline microsite

This approach ensures:

- Users see maintenance page before database changes begin
- Manual intervention points allow for issue resolution
- Automated steps can be paused between stages
- Rollback is possible at each approval gate

### Testing Strategy

Following TDD principles, the microsite has comprehensive test coverage:

- **101 unit tests** using WebApplicationFactory (no mocks)
- Test coverage includes:
  - All HTTP methods return 503
  - Various URL paths and patterns
  - Malicious input handling (path traversal, XSS attempts)
  - Health endpoint returns correct JSON structure
  - Static assets serve correctly
  - Retry-After headers present
  - HTTP security headers (30 dedicated tests):
    - Removal of 30+ server identification headers per OWASP (prevent technology fingerprinting)
    - Presence of all 12 OWASP-recommended protective headers
    - Secure Cache-Control configuration per OWASP spec
    - Cross-origin isolation policies (COEP, COOP, CORP)
    - Prevention of information leakage about technology stack

Tests follow naming convention: `Subject_ContextAndOrScenario_Outcome`

### Alternative Approaches Not Viable

**Why not just a static HTML page?**

- Cannot set HTTP 503 status code
- No control over response headers
- Cannot differentiate health check endpoint
- Poor user experience for search engines and monitoring

**Why not Azure Front Door redirect?**

- Configuration changes currently disabled
- Would require changes during critical maintenance window
- Risk of misconfiguration during high-pressure situation

## Consequences

### Positive

- Clear separation of concerns between main app and maintenance mode
- Infrastructure orchestration remains functional during maintenance
- Search engines and monitoring tools receive appropriate signals
- User experience matches main application design
- Comprehensive test coverage provides confidence
- Manual approval gates prevent automated deployment issues
- Can be tested independently before maintenance window

### Negative

- Additional application to maintain and deploy
- Requires coordination between main app and microsite scaling
- Infrastructure cost of running additional container (minimal, only during maintenance)
- Need to keep GovUK Frontend dependency versions synchronized

### Neutral

- Adds complexity to deployment pipeline but improves safety
- Requires team understanding of when microsite is active vs inactive

### Message Customisation

The microsite supports configurable maintenance messages via environment variables, enabling flexibility in the message displayed without requiring redeployment:

**Configuration Options:**

1. **Default Generic Message** (no configuration required)
   - Displays generic "service temporarily unavailable" messaging
   - Suitable when specific timing is unknown or maintenance is brief
   - Example:

     ```
     You'll be able to use the service later.

     The service is temporarily unavailable for scheduled maintenance.
     This normally lasts only a few minutes. Please check back later.
     ```

2. **Custom Messages** (via environment variables)
   - Configure `Maintenance__MessageParagraphs__*` array in Azure Container Apps
   - Each array element becomes a separate paragraph
   - Enables specific timing and context without code changes
   - Example configuration:

     ```
     Maintenance__MessageParagraphs__0 = "The service will be unavailable from 5pm on Monday 4th November."
     Maintenance__MessageParagraphs__1 = "You will be able to use the service from 9am on Tuesday 5th November."
     ```

**Rationale for Configuration-Based Approach:**

This design addresses the scenario of updating the message mid-deployment (e.g. what if it is taking longer than expected/planned?).

- If database migrations take longer than planned, messages can be updated mid-maintenance
- If issues arise requiring extended downtime, revised timing can be communicated immediately
- No need to rebuild/redeploy container just to change user-facing text
- Different environments (dev/test/production) can have environment-specific messaging
- Maintenance windows can be communicated accurately based on actual progress
- Default fallback ensures users always see appropriate messaging

**Implementation Details:**

- Configuration bound via `IOptions<MaintenanceConfiguration>` pattern
- View renders paragraphs from `MessageParagraphs` list or falls back to defaults
- No code changes required to modify messaging
- See `src/Dfe.PlanTech.Web.SiteOfflineMicrosite/Configuration/MaintenanceConfiguration.cs` for structure

## Future Considerations

1. **Monitoring Integration**
   - Integrate health endpoint with application monitoring
   - Alert on unexpected maintenance mode activation

2. **Azure Front Door Configuration**
   - If/when AFD configuration changes are re-enabled, evaluate whether traffic routing could be simplified
   - Current microsite approach remains valid regardless
