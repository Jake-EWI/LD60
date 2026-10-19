using Godot;

namespace LD60.UI;

public partial class TitleScreen : Control
{
    private Control buttons;
    private Control settings;

    public override void _Ready()
    {
        buttons = GetNode<Control>("Buttons");
        settings = GetNode<Control>("Settings");
        GetNode<HSlider>("Settings/MasterSlider").SetValueNoSignal(AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("Master")));
        GetNode<HSlider>("Settings/MusicSlider").SetValueNoSignal(AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("Music")));
        GetNode<HSlider>("Settings/VoicesSlider").SetValueNoSignal(AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("Voices")));
        GetNode<OptionButton>("Settings/DisplayMode").Selected = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0;
    }

    private void OnNewGamePressed()
    {
    }

    private void OnLoadGamePressed()
    {
    }

    private void OnSettingsPressed()
    {
        buttons.Visible = false;
        settings.Visible = true;
    }

    private void OnBackPressed()
    {
        settings.Visible = false;
        buttons.Visible = true;
    }

    private void OnMasterChanged(double value)
    {
        AudioServer.SetBusVolumeLinear(AudioServer.GetBusIndex("Master"), (float)value);
    }

    private void OnMusicChanged(double value)
    {
        AudioServer.SetBusVolumeLinear(AudioServer.GetBusIndex("Music"), (float)value);
    }

    private void OnVoicesChanged(double value)
    {
        AudioServer.SetBusVolumeLinear(AudioServer.GetBusIndex("Voices"), (float)value);
    }

    private void OnDisplayModeSelected(long index)
    {
        DisplayServer.WindowSetMode(index == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
}
