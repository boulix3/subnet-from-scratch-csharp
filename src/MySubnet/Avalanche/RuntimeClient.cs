namespace MySubnet.Avalanche;
public class RuntimeClient
{
    private Vm.Runtime.Runtime.RuntimeClient _grpcClient;
    public RuntimeClient(Vm.Runtime.Runtime.RuntimeClient grpcClient)
    {
        _grpcClient = grpcClient;
    }
}