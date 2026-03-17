# Project

KubeCtrlUI is a cross-platform desktop UI built on top of `kubectl`. It wraps the Kubernetes C# client to provide a graphical interface for common cluster operations.

**Status**: Early ideation/prototyping — structure and features are still evolving.

## Tech Stack
- **Language**: C# / .NET 10
- **UI**: Avalonia 11 (MVVM, compiled bindings, Fluent theme)
- **Reactive**: ReactiveUI.Avalonia
- **Kubernetes**: KubernetesClient 18 (official C# SDK)
- **Logging**: Microsoft.Extensions.Logging + Debug provider

## Project Structure
```
app/
  Models/          # Plain records/DTOs (KubeContext, KubeNamespace)
  ViewModels/      # ReactiveObject-based ViewModels
  Views/           # Avalonia AXAML views
  Assets/          # Icons, images
```

## Conventions
- Follow MVVM strictly: no business logic in Views or code-behind
- Use ReactiveUI for bindings and commands (`WhenAnyValue`, `ReactiveCommand`)
- Use compiled bindings (`x:CompileBindings="True"`) in AXAML
- Use `App.CreateLogger<T>()` for logging (DI is wired in `App.axaml.cs`)
- Models are immutable C# records

## Build & Run
```bash
cd app
dotnet build
dotnet run
```

## What's Built So Far
- Context list: loads all kubeconfig contexts, shows current with `*`
- Namespace list: loads namespaces for selected context
- Context switching: double-click a context to make it active

## Working with Claude

Always propose a plan before making file changes. Include:
- Files to be modified/created/deleted
- Summary of changes per file
- Risks or alternatives

Do not proceed until the user says "approved", "yes", or "go ahead".
