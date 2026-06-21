namespace SimpleInsuranceApp.Infrastructure;

/// <summary>
/// A tiny Swagger UI page that renders the OpenAPI document produced by the built-in
/// <c>AddOpenApi()</c>/<c>MapOpenApi()</c> services. Using the built-in document keeps
/// the OpenAPI package version pinned to the SDK; the UI assets load from a CDN.
/// </summary>
public static class SwaggerUi
{
    public const string Html = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>Insurance Core API - Swagger UI</title>
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css" />
        </head>
        <body>
            <div id="swagger-ui"></div>
            <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
            <script>
                window.ui = SwaggerUIBundle({
                    url: '/openapi/v1.json',
                    dom_id: '#swagger-ui',
                });
            </script>
        </body>
        </html>
        """;
}
