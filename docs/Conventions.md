# Conventions

## Folder Structure

| Path                | Purpose                          |
| ------------------- | -------------------------------- |
| ./.github/workflows | GitHub Action/Workflow YML Files |
| ./docs              | Documentation                    |
| ./src               | Project source code              |
| ./terraform         | Terraform code                   |
| ./tests             | Test projects                    |

## Code Conventions

- Use **PascalCase** for classes/namespaces/filenames, e.g. Dfe.HtmlFilesSomething.Xml

- use **camelCase** for variables without underscores infixed, e.g. arbitrarySa_User_Id.
  - Under prefixes for class private variables is good, e.g. \_logger
- Be explicit with access modifiers in Interfaces, i.e. use public/private/protected and avoid omitting them assuming the reader understands the default is public.
  - In previous versions of C# omitting public was acceptable when this was the only option, however, later versions of C# provide more options as listed above.

## Frontend Conventions
- All functionality should be accessible for users with javascript disabled.
  - Where it is impossible to do this and the functionality of something on the page is only possible to achieve with javascript, it should:
    - Not be something critical
    - Be hidden when javascript is disabled
  - This can be done by adding the class `js-only` to any elements requiring javascript. This hides them, and at runtime the hidden class and attribute are removed from all `js-only` elements when javascript is enabled.
    For example
    ```html
    <div class="js-only">This element will be hidden by default, but made visible via JS.</div>
    ```
  - The code that unhides these elements is located in [_BodyEnd.cshtml](src/Dfe.PlanTech.Web/Views/Shared/_BodyEnd.cshtml)
