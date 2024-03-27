using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;

string rgName = "cnsvpn_group_02060923";
string vmName = "cnsvpn";

ArmClient armClient = new ArmClient(new DefaultAzureCredential());
SubscriptionResource subscription = armClient.GetDefaultSubscription();

ResourceGroupResource resourceGroup = await subscription.GetResourceGroups().GetAsync(rgName);
VirtualMachineCollection vmCollection = resourceGroup.GetVirtualMachines();
VirtualMachineResource vm = await vmCollection.GetAsync(vmName);

async void Callback(object? _)
{
    await CheckVm(vm);
}

Timer timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

await Task.Delay(Timeout.Infinite);


static async Task CheckVm(VirtualMachineResource vm)
{
    VirtualMachineInstanceView vmView = await vm.InstanceViewAsync();
    var statuses = vmView.Statuses;

    switch (statuses[1].DisplayStatus)
    {
        case "VM running":
            Console.WriteLine($"The instance {vmView.ComputerName} is currently running.");
            break;
        case "VM deallocated":
            Console.WriteLine($"The instance {vmView.ComputerName} is currently deallocated.");
            Console.WriteLine($"Running the {vmView.ComputerName} instance...");
            await vm.PowerOnAsync(WaitUntil.Completed);
            Console.WriteLine($"The instance {vmView.ComputerName} started successfully!");
            break;
    }
}
