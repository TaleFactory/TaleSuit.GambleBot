using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoenixWrapped;
using TaleKit.Extension;
using TaleKit.Game;
using TaleKit.Game.Storage;
using TaleKit.Phoenix;
using TaleKit.Phoenix.Extension;

namespace TaleSuit.GambleBot.Context;

public partial class MainWindowContext : ObservableObject
{
    public ObservableCollection<string> Characters { get; } = [];
    public ObservableCollection<InventoryItem> Items { get; } = [];

    [ObservableProperty]
    private string? selectedCharacter;

    [ObservableProperty]
    private InventoryItem? selectedItem;
    
    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private string logs = string.Empty;

    [ObservableProperty] private bool gambling;
    
    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SelectCharacterCommand { get; }
    public IRelayCommand RefreshItemsCommand { get; }
    public IRelayCommand GambleCommand { get; }

    private Task? task;
    private CancellationTokenSource? cts;
    
    public MainWindowContext()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        SelectCharacterCommand = new AsyncRelayCommand(OnSelectCharacter);
        RefreshItemsCommand = new RelayCommand(OnRefreshItems);
        GambleCommand = new AsyncRelayCommand(OnGamble);
    }

    private Task OnGamble()
    {
        return Task.Run(() =>
        {
            if (Gambling)
            {
                cts?.Cancel();
                task?.Wait();
                
                return;
            }

            cts = new CancellationTokenSource();
            task = Gamble(cts.Token);

            Gambling = true;
        });
    }

    private async Task Gamble(CancellationToken token)
    {
        if (Session == null)
        {
            return;
        }

        if (SelectedItem == null)
        {
            return;
        }

        var slot = SelectedItem.Slot;
        while (!token.IsCancellationRequested)
        {
            var equipment = Session.Character.Inventory.GetItem(InventoryType.Equipment, slot);
            if (equipment.Rarity >= 7)
            {
                Log("Rarity reached, stopping gambling");
                break;
            }
                    
            var equippedAmulet = Session.Character.Equipment.GetItem(EquipmentSlot.Amulet);
            if (equippedAmulet is not { VirtualNumber: 4262 })
            {
                var unsealedAmulet = Session.Character.Inventory.GetItem(x => x.VirtualNumber == 4262);
                if (unsealedAmulet == null)
                {
                    var sealedAmulet = Session.Character.Inventory.GetItem(x => x.VirtualNumber == 5735);
                    if (sealedAmulet == null)
                    {
                        Log("No more amulet found, stopping gambling");
                        break;
                    }

                    Log("Unsealing amulet...");
                    Session.Character.Inventory.Use(sealedAmulet);

                    unsealedAmulet = await Session
                        .Character
                        .Inventory
                        .WaitForItem(x => x.VirtualNumber == 4262);
                    
                    await Task.Delay(1000);
                }
                        
                Log("Equipping gambling amulet");
                Session.Character.Equipment.Wear(unsealedAmulet);
                
                await Task.Delay(1000);
            }
            
            Log("Gambling...");
            await Session.Character.Dance(-2, 5000).ThenExecute(() =>
            {
                Session.SendPacket($"up_gr 7 0 {slot} 0");
            });

            await Task.Delay(2000);
        }

        Log("Gambling stopped");
        Gambling = false;
    }
    
    private void OnLoaded()
    {
        Characters.Clear();
        
        var windows = PhoenixClientFactory.GetWindows();
        foreach (var window in windows)
        {
            Characters.Add(window.Character);
        }
    }

    private void OnRefreshItems()
    {
        if (Session == null)
        {
            return;
        }
        
        Items.Clear();
        
        var items = Session
            .Character
            .Inventory
            .GetItems(InventoryType.Equipment)
            .OrderBy(x => x.Slot);
        
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }
    
    private async Task OnSelectCharacter()
    {
        if (SelectedCharacter == null)
        {
            return;
        }

        var createdSession = PhoenixFactory.CreateSession(SelectedCharacter);
        while (!createdSession.IsReady())
        {
            await Task.Delay(100);
        }

        var items = createdSession
            .Character
            .Inventory
            .GetItems(InventoryType.Equipment)
            .OrderBy(x => x.Slot);
        
        foreach (var item in items)
        {
            Items.Add(item);
        }
        
        Session = createdSession;
    }

    private void Log(string message)
    {
        Logs += message + Environment.NewLine;
    }
}