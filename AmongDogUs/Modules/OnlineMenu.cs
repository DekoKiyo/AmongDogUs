// Taken from https://github.com/NuclearPowered/Reactor/, licensed under the LGPLv3
// Source Code from TheOtherRoles(https://github.com/Eisbison/TheOtherRoles)

// Edited by DekoKiyo

namespace AmongDogUs.Modules;

internal static class OnlineMenu
{
    internal static void Initialize()
    {
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
        {
            if (!scene.name.Equals("MMOnline")) return;
            FixPrivateMenuPos();
            if (!TryMoveObjects()) return;

            var editName = FastDestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
            var nameText = Object.Instantiate(editName.nameText.gameObject);

            nameText.transform.localPosition += Vector3.up * 1.8f;

            var textBox = nameText.GetComponent<TextBoxTMP>();
            textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
            textBox.outputText.transform.position = nameText.transform.position;
            textBox.outputText.fontSize = 4f;

            textBox.OnChange.AddListener((Action)(() =>
            {
                DataManager.Player.Customization.name = textBox.text;
            }));
            textBox.OnEnter = textBox.OnFocusLost = textBox.OnChange;

            textBox.Pipe.GetComponent<TextMeshPro>().fontSize = 4f;
        }));
    }

    private static bool TryMoveObjects()
    {
        var toMove = new string[] { "HostGameButton", "FindGameButton", "JoinGameButton" };

        var yOffset = Vector3.down * 1.5f;
        var yConst = Vector3.up * 0.7f;

        var gameObjects = toMove.Select(x => GameObject.Find($"NormalMenu/Buttons/{x}")).ToList();
        if (gameObjects.Any(x => x is null))
        {
            Main.Logger.Log(LogLevel.Info, "Disabled Free Name");
            return false;
        }

        for (var i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].transform.position = (yOffset * i) + yConst;
        }

        return true;
    }

    internal static void DisableOnline(VersionShower vs)
    {
        try
        {
            var FindGameButtonBase = GameObject.Find("NormalMenu/Buttons/FindGameButton");
            if (FindGameButtonBase is not null)
            {
                var FindGameButton = FindGameButtonBase.transform.FindChild("FindGameButton").gameObject;
                if (FindGameButton is not null)
                {
                    FindGameButton.active = false;

                    var DisableText = Object.Instantiate(vs.text, FindGameButtonBase.transform);
                    DisableText.text = string.Format(ModResources.DisableOnlineText);
                    DisableText.transform.localPosition = new Vector3(0.1037f, -0.283f, 0f);
                    DisableText.gameObject.active = true;
                }
            }
        }
        catch (Exception)
        {
            Main.Logger.Log(LogLevel.Warning, "Error on Disable Online");
        }
    }

    private static void FixPrivateMenuPos()
    {
        var JoinGameMenu = GameObject.Find("NormalMenu/Buttons/JoinGameButton/JoinGameMenu");
        JoinGameMenu.transform.localPosition += new Vector3(0f, 0.5f, 0f);
    }
}