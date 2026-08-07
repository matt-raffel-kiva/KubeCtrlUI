namespace KubeCtrlUI.Models;

public record KubeContext(
    string Name,
    string Cluster,
    string AuthInfo,
    string Namespace,
    bool IsCurrent);
    
public record KubeNamespace(
    string Name,
    bool IsCurrent);

public record KubePod(
    string Name,
    string Phase,
    string Ready,
    int RestartCount,
    string Age);