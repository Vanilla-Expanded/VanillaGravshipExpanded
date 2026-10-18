using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{

    public class Window_RenameAsteroid : Window
    {

        public WorldObject worldObject;
        public Pawn pawn;

        public override Vector2 InitialSize => new Vector2(500f, 380f);
        private Vector2 scrollPosition = new Vector2(0, 0);

        private int startAcceptingInputAtFrame;
        private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;
        private bool focusedRenameField;
        protected virtual int MaxNameLength => 60;
        protected string curName;


        private static readonly Color borderColor = new Color(0.13f, 0.13f, 0.13f);
        private static readonly Color fillColor = new Color(0, 0, 0, 0.1f);

        public Window_RenameAsteroid(WorldObject worldObject, Pawn pawn)
        {
            draggable = false;
            resizeable = false;
            preventCameraMotion = false;
            doCloseX = true;
            forcePause = true;
            closeOnAccept = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            this.worldObject = worldObject;
            this.pawn = pawn;
            curName = worldObject.Label;
        }

        public AcceptanceReport NameIsValid(string name)
        {
            return name.Length != 0;
        }

        public void WasOpenedByHotkey()
        {
            startAcceptingInputAtFrame = Time.frameCount + 1;
        }



        public override void DoWindowContents(Rect inRect)
        {

            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                flag = true;
                Event.current.Use();
            }
            Rect rect = new Rect(inRect);
            Text.Font = GameFont.Medium;
            rect.height = Text.LineHeight + 10f;
         
            var IntroLabel = new Rect(0, 0, 300, 32f);
            Widgets.Label(IntroLabel, "VGE_RenameAsteroid".Translate().CapitalizeFirst());
            Text.Font = GameFont.Small;
            var IntroLabel2 = new Rect(0, 40, 450, 72f);
            Widgets.Label(IntroLabel2, "VGE_RenameAsteroidDesc".Translate(pawn.NameFullColored, worldObject.def.LabelCap));
           
            Text.Font = GameFont.Small;
            GUI.SetNextControlName("RenameField");
            string text = Widgets.TextField(new Rect(0f, rect.height+80, inRect.width, 35f), curName);
            if (AcceptsInput && text.Length < MaxNameLength)
            {
                curName = text;
            }
            else if (!AcceptsInput)
            {
                ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();
            }
            if (!focusedRenameField)
            {
                UI.FocusControl("RenameField", this);
                focusedRenameField = true;
            }
            if (!(Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 10f, inRect.width - 15f - 15f, 35f), "OK") || flag))
            {
                return;
            }
            AcceptanceReport acceptanceReport = NameIsValid(curName);
            if (!acceptanceReport.Accepted)
            {
                if (acceptanceReport.Reason.NullOrEmpty())
                {
                    Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
                }
                return;
            }

            if (worldObject is SpaceMapParent spaceMapParent)
            {
                spaceMapParent.Name = text;
            }

            Find.WindowStack.TryRemove(this);



        }
    }
}
