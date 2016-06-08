using KSPPluginFramework;
using UnityEngine;

/** UGLY **/

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class GameframerSkin : MonoBehaviourExtended
    {
        public GUIStyle subduedContentStyle;
        public GUIStyle dividerStyle;
        private GUIStyle foobarStyle;
        private GUIStyle bottomLabelForButtonStyle;
        private GUIStyle errorStyle;
        private GUIStyle newMainStyle;
        private GUIStyle subduedStyle;
        private GUIStyle subdued2Style;
        private GUIStyle smallXStyle;
        private GUIStyle listLabelStyle;
        private GUIStyle listMETLabelStyle;
        private GUIStyle toggleStyle;
        private GUIStyle bigLabelStyle;
        private GUIStyle contentStyle;
        private GUIStyle headerStyle;
        private GUIStyle missionStyle;
        private GUIStyle BillboardContentStyle;
        private GUIStyle redButtonStyle;
        private GUIStyle normalBoxStyle;
        private GUIStyle CustomTooltip;
        private GUIStyle missionContentStyle;
        private GUIStyle missionHeaderStyle;
        private GUIStyle missionHeaderStyle2;
        private GUIStyle initIconStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle wrappedTextFieldStyle;
        private GUIStyle whiteRightLabelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle disclaimerLabelStyle;
        private GUIStyle bigGreenLabelStyle;

        private Texture2D grayTexture;
        private Texture2D clearTexture;
        private Texture2D redTexture;
        private Color clearBlack;

        internal override void Awake()
        {
        }

        private void AllStyles()
        {
            dividerStyle = new GUIStyle();
            dividerStyle.name = "DividerStyle";
            dividerStyle.normal.background = grayTexture;
            dividerStyle.stretchWidth = true;
            dividerStyle.fixedHeight = 1;
            dividerStyle.margin = new RectOffset(0, 0, 1, 1);

            foobarStyle = new GUIStyle(GUI.skin.label);
            foobarStyle.name = "FoobarStyle";
            foobarStyle.alignment = TextAnchor.MiddleLeft;
            foobarStyle.normal.textColor = new Color(255, 255, 255);
            foobarStyle.fontSize = 14;

            errorStyle = new GUIStyle(GUI.skin.label);
            errorStyle.name = "ErrorText";
            errorStyle.normal.textColor = errorStyle.onNormal.textColor = Color.white;
            errorStyle.fontSize = 16;
            errorStyle.padding = new RectOffset(20, 20, 20, 20);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, errorStyle);
            newMainStyle = new GUIStyle(GUI.skin.label);
            newMainStyle.name = "NewBigText";
            newMainStyle.normal.textColor = new Color(255, 255, 255);
            newMainStyle.fontSize = 14;
            newMainStyle.wordWrap = true;
            newMainStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, newMainStyle);
            
            subduedStyle = new GUIStyle(GUI.skin.label);
            subduedStyle.name = "SubduedText";
            subduedStyle.normal.textColor = new Color(255f, 255f, 255f, 0.5f);
            subduedStyle.fontSize = 14;
            subduedStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, subduedStyle);

            subdued2Style = new GUIStyle(GUI.skin.label);
            subdued2Style.name = "Subdued2Text";
            subdued2Style.normal.textColor = new Color(255f, 255f, 255f, 0.5f);
            subdued2Style.fontSize = 14;
            subdued2Style.alignment = TextAnchor.LowerRight;
            subdued2Style.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, subdued2Style);

            smallXStyle = new GUIStyle(GUI.skin.button);
            smallXStyle.name = "SmallXButton";
            smallXStyle.normal.textColor = Color.red;
            smallXStyle.hover.textColor = Color.red;
            smallXStyle.fixedWidth = smallXStyle.fixedHeight = 24;
            smallXStyle.normal.background = smallXStyle.onNormal.background = clearTexture;
            smallXStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, smallXStyle);

            listLabelStyle = new GUIStyle(GUI.skin.label);
            listLabelStyle.name = "ListText";
            listLabelStyle.normal.textColor = listLabelStyle.onNormal.textColor = Color.white;
            listLabelStyle.fontSize = 14;
            listLabelStyle.padding = new RectOffset(0, 0, 4, 0);
            listLabelStyle.alignment = TextAnchor.MiddleLeft;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, listLabelStyle);

            listMETLabelStyle = new GUIStyle(GUI.skin.label);
            listMETLabelStyle.name = "ListMETText";
            listMETLabelStyle.normal.textColor = listMETLabelStyle.onNormal.textColor = Color.gray;
            listMETLabelStyle.fontSize = 14;
            listMETLabelStyle.fixedWidth = 40;
            listMETLabelStyle.clipping = TextClipping.Clip;
            listMETLabelStyle.wordWrap = false;
            listMETLabelStyle.alignment = TextAnchor.MiddleRight;
            listMETLabelStyle.padding = new RectOffset(0, 0, 4, 4);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, listMETLabelStyle);

            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.name = "ToggleStyle";
            toggleStyle.fontSize = 14;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, toggleStyle);

            bigLabelStyle = new GUIStyle(GUI.skin.label);
            bigLabelStyle.name = "UpgradeText";
            bigLabelStyle.normal.textColor = bigLabelStyle.active.textColor = Color.white;
            bigLabelStyle.fontSize = 14;
            bigLabelStyle.padding = new RectOffset(4, 0, 4, 4);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, bigLabelStyle);

            missionStyle = new GUIStyle(GUI.skin.label);
            missionStyle.name = "WelcomeText";
            missionStyle.normal.textColor = new Color(255, 255, 255);
            missionStyle.fontSize = 18;
            missionStyle.alignment = TextAnchor.MiddleLeft;
            missionStyle.wordWrap = true;
            missionStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, missionStyle);

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.name = "HeaderStyle";
            headerStyle.normal.textColor = new Color(255, 255, 255);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.wordWrap = true;
            headerStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, headerStyle);

            contentStyle = new GUIStyle(GUI.skin.label);
            contentStyle.name = "ContentStyle";
            contentStyle.normal.textColor = new Color(255, 255, 255);
            contentStyle.fontSize = 12;
            contentStyle.alignment = TextAnchor.MiddleLeft;
            contentStyle.wordWrap = true;
            contentStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, contentStyle);

            subduedContentStyle = new GUIStyle(GUI.skin.label);
            subduedContentStyle.name = "SubduedContentStyle";
            subduedContentStyle.normal.textColor = new Color(255f, 255f, 255f, 0.3f);
            subduedContentStyle.fontSize = 12;
            subduedContentStyle.alignment = TextAnchor.MiddleLeft;
            subduedContentStyle.wordWrap = true;
            subduedContentStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, subduedContentStyle);

            missionHeaderStyle = new GUIStyle(GUI.skin.label);
            missionHeaderStyle.name = "InitQuestion";
            missionHeaderStyle.alignment = TextAnchor.MiddleLeft;
            missionHeaderStyle.wordWrap = true;
            missionHeaderStyle.stretchWidth = true;
            missionHeaderStyle.fontStyle = FontStyle.Bold;
            missionHeaderStyle.normal.textColor = new Color(255, 255, 255);
            missionHeaderStyle.fontSize = 18;
            missionHeaderStyle.padding = new RectOffset(0, 0, 6, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, missionHeaderStyle);

            missionHeaderStyle2 = new GUIStyle(GUI.skin.label);
            missionHeaderStyle2.name = "FormHeader";
            missionHeaderStyle2.alignment = TextAnchor.LowerLeft;
            missionHeaderStyle2.wordWrap = false;
            missionHeaderStyle2.fontStyle = FontStyle.Bold;
            missionHeaderStyle2.normal.textColor = new Color(255, 255, 255);
            missionHeaderStyle2.fontSize = 18;
            missionHeaderStyle2.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, missionHeaderStyle2);

            missionContentStyle = new GUIStyle(GUI.skin.label);
            missionContentStyle.name = "MissionContent";
            missionContentStyle.normal.textColor = new Color(255, 255, 255);
            missionContentStyle.fontSize = 18;
            missionContentStyle.wordWrap = true;
            missionContentStyle.alignment = TextAnchor.LowerLeft;
            missionContentStyle.padding = new RectOffset(0, 0, 0, 0);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, missionContentStyle);

            redButtonStyle = new GUIStyle(SkinsLibrary.DefKSPSkin.button);
            redButtonStyle.normal.background = redTexture;
            redButtonStyle.name = "RedButtonStyle";
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, redButtonStyle);

            BillboardContentStyle = new GUIStyle(GUI.skin.label);
            BillboardContentStyle.name = "BillboardContent";
            BillboardContentStyle.normal.textColor = new Color(255, 255, 255);
            BillboardContentStyle.fontSize = 16;
            BillboardContentStyle.wordWrap = true;
            BillboardContentStyle.alignment = TextAnchor.MiddleCenter;
            BillboardContentStyle.padding = new RectOffset(10, 10, 10, 10);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, BillboardContentStyle);

            bottomLabelForButtonStyle = new GUIStyle(GUI.skin.label);
            bottomLabelForButtonStyle.name = "ButtonBottomLabel";
            bottomLabelForButtonStyle.normal.textColor = new Color(205, 205, 205);
            bottomLabelForButtonStyle.fontSize = 13;
            bottomLabelForButtonStyle.alignment = TextAnchor.MiddleLeft;
            bottomLabelForButtonStyle.wordWrap = true;
            bottomLabelForButtonStyle.padding = new RectOffset(2, 2, 8, 2);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, bottomLabelForButtonStyle);

            whiteRightLabelStyle = new GUIStyle(GUI.skin.label);
            whiteRightLabelStyle.name = "WhiteRightLabelStyle";
            whiteRightLabelStyle.normal.textColor = new Color(205, 205, 205);
            whiteRightLabelStyle.fontSize = 13;
            whiteRightLabelStyle.stretchWidth = false;
            whiteRightLabelStyle.alignment = TextAnchor.MiddleRight;
            whiteRightLabelStyle.wordWrap = false;
            whiteRightLabelStyle.padding = new RectOffset(2, 2, 8, 2);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, whiteRightLabelStyle);

            normalBoxStyle = new GUIStyle(GUI.skin.box);
            normalBoxStyle.name = "NormalBox";
            normalBoxStyle.alignment = TextAnchor.MiddleCenter;
            normalBoxStyle.padding = new RectOffset(10, 10, 10, 10);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, normalBoxStyle);

            initIconStyle = new GUIStyle(GUI.skin.label);
            initIconStyle.fixedHeight = initIconStyle.fixedWidth = 38;
            initIconStyle.name = "InitIconStyle";
            initIconStyle.alignment = TextAnchor.MiddleCenter;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, initIconStyle);

            textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.name = "TextFieldStyle";
            textFieldStyle.alignment = TextAnchor.LowerLeft;
            textFieldStyle.stretchWidth = true;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, textFieldStyle);

            wrappedTextFieldStyle = new GUIStyle(GUI.skin.textField);
            wrappedTextFieldStyle.name = "WrappedTextField";
            wrappedTextFieldStyle.alignment = TextAnchor.UpperLeft;
            wrappedTextFieldStyle.stretchWidth = true;
            wrappedTextFieldStyle.wordWrap = true;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, wrappedTextFieldStyle);



            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.name = "ButtonStyle";
            buttonStyle.fontSize = 18;
            buttonStyle.fontStyle = FontStyle.Normal;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.padding = new RectOffset(8, 8, 8, 8);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, buttonStyle);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.name = "LabelStyle";
            labelStyle.fontSize = 18;
            labelStyle.onNormal.textColor = Color.yellow;
            labelStyle.padding = new RectOffset(0, 0, 4, 4);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, labelStyle);

            disclaimerLabelStyle = new GUIStyle(GUI.skin.label);
            disclaimerLabelStyle.name = "DisclaimerStyle";
            disclaimerLabelStyle.fontSize = 18;
            disclaimerLabelStyle.alignment = TextAnchor.MiddleLeft;
            disclaimerLabelStyle.normal.textColor = Color.yellow;
            disclaimerLabelStyle.padding = new RectOffset(20, 20, 4, 4);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, disclaimerLabelStyle);

            bigGreenLabelStyle = new GUIStyle(GUI.skin.label);
            bigGreenLabelStyle.name = "BigGreenLabel";
            bigGreenLabelStyle.normal.textColor = bigGreenLabelStyle.active.textColor = Color.green;
            bigGreenLabelStyle.fontSize = 20;
            bigGreenLabelStyle.padding = new RectOffset(0, 0, 4, 4);
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, bigGreenLabelStyle);
        }

        internal override void OnGUIOnceOnly()
        {
            clearTexture = new Texture2D(1, 1);
            clearBlack = new Color(0f, 0f, 0f, 0f);
            clearTexture.SetPixel(0, 0, clearBlack);
            clearTexture.Apply();

            redTexture = new Texture2D(1, 1);
            redTexture.SetPixel(0, 0, new Color(244f, 0f, 0f, 0.75f));
            redTexture.Apply();

            CustomTooltip = new GUIStyle();
            CustomTooltip.name = "Tooltip";
            CustomTooltip.padding = new RectOffset(15, 15, 15, 15);
            CustomTooltip.margin = new RectOffset(40, 0, 40, 0);
            CustomTooltip.normal.textColor = Color.white;
            Texture2D tooltipTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            var pixels4 = tooltipTex.GetPixels32();
            for (int i = 0; i < pixels4.Length; ++i)
            {
                pixels4[i].r = 0;
                pixels4[i].g = 0;
                pixels4[i].b = 0;
                pixels4[i].a = 128;
            }
            tooltipTex.SetPixels32(pixels4); tooltipTex.Apply();
            CustomTooltip.normal.background = tooltipTex;
            SkinsLibrary.AddStyle(SkinsLibrary.DefSkinType.KSP, CustomTooltip);

            grayTexture = new Texture2D(1, 1);
            grayTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
            grayTexture.Apply();

            AllStyles();
        }
    }
}
