# Conventions

## Folder Structure

| Path        | Purpose                       |
| ----------- | ----------------------------- |
| ./src       | Project source code           |
| ./tests     | Test projects                 |
| ./pipelines | DevOps files / GitHub Actions |
| ./docs      | Documentation                 |
| ./terraform | Terraform code                |

## Code Conventions

- Use **PascalCase** for classes/namespaces/filenames, e.g. Dfe.HtmlFilesSomething.Xml

- use **camelCase** for variables without underscores infixed, e.g. arbitrarySa_User_Id.
  - Under prefixes for class private variables is good, e.g. \_logger
- Be explicit with access modifiers in Interfaces, i.e. use public/private/protected and avoid omitting them assuming the reader understands the default is public.
  - In previous versions of C# omitting public was acceptable when this was the only option, however, later versions of C# provide more options as listed above.
