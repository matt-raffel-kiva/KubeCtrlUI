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