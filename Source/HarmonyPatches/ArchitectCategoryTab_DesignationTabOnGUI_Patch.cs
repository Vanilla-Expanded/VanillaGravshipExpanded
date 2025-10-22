using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(ArchitectCategoryTab), nameof(ArchitectCategoryTab.DesignationTabOnGUI))]
    public static class ArchitectCategoryTab_DesignationTabOnGUI_Patch
    {
        private static ArchitectCategoryTab currentArchitectCategoryTab;
        private static readonly Dictionary<DesignationCategoryDef, DesignationCategoryDef> selectedCategory = new Dictionary<DesignationCategoryDef, DesignationCategoryDef>();
        private static readonly Dictionary<DesignationCategoryDef, bool> categorySearchMatches = new Dictionary<DesignationCategoryDef, bool>();
        private static Vector2 leftPanelScrollPosition, designatorGridScrollPosition, ordersScrollPosition;
        private static DesignationCategoryDef lastMainCategory;
        private static string lastSearchText = "";
        public static bool Prepare()
        {
            return !ModsConfig.IsActive("ferny.BetterArchitect");
        }

        public static void Reset()
        {
            selectedCategory.Clear();
            categorySearchMatches.Clear();
            lastMainCategory = null;
            lastSearchText = "";
            leftPanelScrollPosition = designatorGridScrollPosition = ordersScrollPosition = Vector2.zero;
            currentArchitectCategoryTab = null;
        }

        private static (List<Designator> buildables, List<Designator> orders) SeparateDesignatorsByType(IEnumerable<Designator> allDesignators, DesignationCategoryDef category)
        {
            var buildables = new List<Designator>();
            var orders = new List<Designator>();
            foreach (var designator in allDesignators)
            {
                if (designator is Designator_Build || (designator is Designator_Dropdown dd && dd.Elements.Any(e => e is Designator_Build)))
                {
                    buildables.Add(designator);
                }
                else
                {
                    orders.Add(designator);
                }
            }
            return (buildables, orders);
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(ArchitectCategoryTab __instance)
        {
            if (__instance.def != VGEDefOf.Odyssey)
            {
                return true;
            }

            currentArchitectCategoryTab = __instance;
            DrawBetterArchitectMenu(__instance);
            if (lastMainCategory != __instance.def)
            {
                leftPanelScrollPosition = designatorGridScrollPosition = ordersScrollPosition = Vector2.zero;
                lastMainCategory = __instance.def;
            }
            return false;
        }

        private static void DrawBetterArchitectMenu(ArchitectCategoryTab tab)
        {
            if (Find.DesignatorManager.SelectedDesignator != null)
            {
                Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35) - ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).WinHeight - 270f);
            }
            var menuHeight = 330f;
            var leftWidth = 200f;
            var ordersWidth = 175f;
            var gizmoSize = 75f;
            var gizmoSpacing = 5f;
            var availableWidth = UI.screenWidth - 195f - (((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).RequestedTabSize.x + 10f) - leftWidth - ordersWidth;
            var gizmosPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / (gizmoSize + gizmoSpacing)));
            var gridWidth = gizmosPerRow * (gizmoSize + gizmoSpacing) + gizmoSpacing + 11;
            var mainRect = new Rect(
                ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).RequestedTabSize.x + 10f,
                UI.screenHeight - menuHeight - 35f,
                leftWidth + gridWidth + ordersWidth,
                menuHeight);
            var leftRect = new Rect(mainRect.x, mainRect.y + 20f, leftWidth, mainRect.height - 30f);
            var gridRect = new Rect(leftRect.xMax, mainRect.y, gridWidth, mainRect.height);
            var ordersRect = new Rect(gridRect.xMax, mainRect.y + 30f, ordersWidth, mainRect.height - 30f);
            var newColor = new Color(Color.white.r, Color.white.g, Color.white.b, 1f - 0.42f);
            Widgets.DrawWindowBackground(mainRect, newColor);
            
            var allCategories = DefDatabase<DesignationCategoryDef>.AllDefsListForReading
                .Where(d => d.GetModExtension<NestedCategoryExtension>()?.parentCategory == tab.def).ToList();
            allCategories.Add(tab.def);
            
            var designatorDataList = new List<DesignatorCategoryData>();
            foreach (var cat in allCategories)
            {
                var allDesignators = cat.ResolvedAllowedDesignators.Where(d => d.Visible).ToList();
                var (buildables, orders) = SeparateDesignatorsByType(allDesignators, cat);
                designatorDataList.Add(new DesignatorCategoryData(cat, cat == tab.def, allDesignators, buildables, orders));
            }

            List<Designator> designatorsToDisplay;
            List<Designator> orderDesignators;
            DesignationCategoryDef category;
            DesignationCategoryDef selectedCategoryResult = HandleCategorySelection(leftRect, tab.def, designatorDataList);
            var selectedData = designatorDataList.FirstOrDefault(d => d.def == selectedCategoryResult);
            designatorsToDisplay = selectedData.buildables;
            orderDesignators = selectedData.orders;
            category = selectedCategoryResult;

            var mouseoverGizmo = DrawDesignatorGrid(gridRect, category, designatorsToDisplay);
            var orderGizmo = DrawOrdersPanel(ordersRect, orderDesignators);
            if (orderGizmo != null) mouseoverGizmo = orderGizmo;
            DoInfoBox(mouseoverGizmo ?? Find.DesignatorManager.SelectedDesignator);
            if (Event.current.type == EventType.MouseDown && Mouse.IsOver(mainRect)) Event.current.Use();
        }

        private static DesignationCategoryDef HandleCategorySelection(Rect rect, DesignationCategoryDef mainCat, List<DesignatorCategoryData> designatorDataList)
        {
            var allCategories = designatorDataList.Select(d => d.def).ToList();
            DesignationCategoryDef currentSelection = null;
            
            if (lastMainCategory == mainCat)
            {
                selectedCategory.TryGetValue(mainCat, out currentSelection);
            }
            
            string currentSearchText = currentArchitectCategoryTab?.quickSearchFilter?.Active == true ? currentArchitectCategoryTab.quickSearchFilter.Text : "";
            if (currentSearchText != lastSearchText)
            {
                lastSearchText = currentSearchText;
                categorySearchMatches.Clear();
                foreach (var cat in allCategories)
                {
                    bool hasSearchMatches = false;
                    if (currentArchitectCategoryTab?.quickSearchFilter?.Active == true)
                    {
                        var categoryData = designatorDataList.FirstOrDefault(d => d.def == cat);
                        if (categoryData != null)
                        {
                            hasSearchMatches = categoryData.allDesignators.Any(MatchesSearch);
                        }
                    }
                    categorySearchMatches[cat] = hasSearchMatches;
                }
                if (currentArchitectCategoryTab?.quickSearchFilter?.Active == true)
                {
                    if (currentSelection != null)
                    {
                        categorySearchMatches.TryGetValue(currentSelection, out bool selectedHasMatches);
                        if (!selectedHasMatches)
                        {
                            var newSelection = allCategories.FirstOrDefault(c => c != null && categorySearchMatches.TryGetValue(c, out var hasMatch) && hasMatch);
                            if (newSelection != null)
                            {
                                currentSelection = newSelection;
                                if (mainCat != null)
                                {
                                    selectedCategory[mainCat] = newSelection;
                                }
                            }
                        }
                    }
                }
            }
            
            var mainCategoryData = designatorDataList.FirstOrDefault(d => d.def == mainCat);
            var mainCategoryHasDesignators = mainCategoryData != null && mainCategoryData.buildables.Any(x => x is Designator_Place || x is Designator_Dropdown);
            var subCategories = allCategories.Where(c => c != mainCat).ToList();
            var filteredSubCategories = new List<DesignationCategoryDef>();

            foreach (var cat in subCategories)
            {
                var categoryData = designatorDataList.FirstOrDefault(d => d.def == cat);
                if (categoryData != null && !categoryData.buildables.NullOrEmpty())
                {
                    filteredSubCategories.Add(cat);
                }
            }

            filteredSubCategories = filteredSubCategories.OrderByDescending(cat => cat.order).ToList();
            bool shouldHideMoreCategory = false;
            if (filteredSubCategories.Any())
            {
                bool subCategoriesHaveBuildings = filteredSubCategories.Any(cat =>
                {
                    var categoryData = designatorDataList.FirstOrDefault(d => d.def == cat);
                    return categoryData != null && !categoryData.buildables.NullOrEmpty();
                });

                shouldHideMoreCategory = subCategoriesHaveBuildings && !mainCategoryHasDesignators;
            }
            if (shouldHideMoreCategory) allCategories.Remove(mainCat);
            var displayCategories = new List<DesignationCategoryDef>();
            displayCategories.AddRange(filteredSubCategories);
            if (!shouldHideMoreCategory) displayCategories.Add(mainCat);
            if (currentSelection == null || !allCategories.Contains(currentSelection))
            {
                if (displayCategories.Any())
                {
                    currentSelection = displayCategories.First();
                }
                else
                {
                    currentSelection = mainCat;
                }
                if (mainCat != null)
                {
                    selectedCategory[mainCat] = currentSelection;
                }
            }
            
            var outRect = rect.ContractedBy(10f);
            var viewRect = new Rect(0, 0, outRect.width - 16f, GetCategoryViewHeight(displayCategories.Count));
            HandleScrollBar(outRect, viewRect, ref leftPanelScrollPosition);
            Widgets.BeginScrollView(outRect, ref leftPanelScrollPosition, viewRect);
            float curY = 0;

            foreach (var cat in displayCategories)
            {
                var rowRect = new Rect(0, curY, viewRect.width, 36);
                bool isSelected = currentSelection == cat;
                bool categoryHasSearchMatches = false;
                if (cat != null)
                {
                    categorySearchMatches.TryGetValue(cat, out categoryHasSearchMatches);
                }

                DrawOptionBackground(rowRect, isSelected, categoryHasSearchMatches, !categoryHasSearchMatches && currentArchitectCategoryTab?.quickSearchFilter?.Active == true);
                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (!isSelected) SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    if (mainCat != null && cat != null)
                    {
                        selectedCategory[mainCat] = cat;
                    }
                    currentSelection = cat;
                }
                string label = cat.LabelCap;
                if (cat == mainCat && filteredSubCategories.Any())
                {
                    label = "VGE_More".Translate();
                }

                var iconRect = new Rect(rowRect.x + 4f, rowRect.y + 8f, 20f, 20f);
                var nestedExtension = cat.GetModExtension<NestedCategoryExtension>();
                if (nestedExtension != null && !string.IsNullOrEmpty(nestedExtension.iconTexPath))
                {
                    var icon = ContentFinder<Texture2D>.Get(nestedExtension.iconTexPath, false);
                    if (icon != null)
                    {
                        Widgets.DrawTextureFitted(iconRect, icon, 1f);
                    }
                }
                Text.Font = GameFont.Small;
                var labelRect = new Rect(iconRect.xMax + 8f, rowRect.y, rowRect.width - iconRect.width - 16f, rowRect.height);

                Text.Anchor = TextAnchor.MiddleLeft; Widgets.Label(labelRect, label); Text.Anchor = TextAnchor.UpperLeft;
                curY += rowRect.height + 5;
            }
            Widgets.EndScrollView();
            return currentSelection;
        }

        private static Designator DrawDesignatorGrid(Rect rect, DesignationCategoryDef category, List<Designator> designators)
        {
            var outRect = new Rect(rect.x, rect.y + 30f, rect.width, rect.height - 30f);
            if (designators.NullOrEmpty())
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                var message = "VGE_NoAvailableDesignators".Translate();
                var textSize = Text.CalcSize(message);
                var messageRect = new Rect(
                    outRect.x + (outRect.width - textSize.x) / 2,
                    outRect.y + (outRect.height - textSize.y) / 2,
                    textSize.x,
                    textSize.y
                );
                Widgets.Label(messageRect, message);
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                return null;
            }
            return DrawFlatGrid(outRect, designators);
        }

        private static Designator DrawFlatGrid(Rect rect, List<Designator> designators)
        {
            var gizmoSize = 75f;
            var gizmoSpacing = 5f;
            var availableWidth = rect.width - 16f;
            var gizmosPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / (gizmoSize + gizmoSpacing)));
            var rowCount = Mathf.CeilToInt((float)designators.Count / gizmosPerRow);
            var rowHeight = gizmoSize + gizmoSpacing + 5f;
            var viewRect = new Rect(0, 0, rect.width - 16f, rowCount * rowHeight).ExpandedBy(3f);
            viewRect.x += 1f;
            viewRect.width -= 7f;
            Designator mouseoverGizmo = null;
            Designator interactedGizmo = null;
            Designator floatMenuGizmo = null;
            Event interactedEvent = null;
            HandleScrollBar(rect, viewRect, ref designatorGridScrollPosition);
            Widgets.BeginScrollView(rect, ref designatorGridScrollPosition, viewRect);
            GizmoGridDrawer.drawnHotKeys.Clear();
            
            for (int i = 0; i < designators.Count; i++)
            {
                int row = i / gizmosPerRow;
                int col = i % gizmosPerRow;
                var designator = designators[i];
                var parms = new GizmoRenderParms
                {
                    highLight = ShouldHighLightGizmo(designator),
                    lowLight = ShouldLowLightGizmo(designator),
                    isFirst = i == 0,
                    multipleSelected = false
                };

                var result = designator.GizmoOnGUI(new Vector2(col * (gizmoSize + gizmoSpacing), row * rowHeight), gizmoSize, parms);
                ProcessGizmoResult(result, designator, ref mouseoverGizmo, ref interactedGizmo, ref floatMenuGizmo, ref interactedEvent);
            }
            ProcessGizmoInteractions(interactedGizmo, floatMenuGizmo, interactedEvent);
            
            Widgets.EndScrollView();
            return mouseoverGizmo;
        }

        private static Designator DrawOrdersPanel(Rect rect, List<Designator> designators)
        {
            var gizmoSize = 75f;
            var gizmoSpacing = 5f;
            var rowHeight = gizmoSize + gizmoSpacing + 5f;
            var outRect = rect.ContractedBy(2f);
            outRect.width += 2;

            var columns = 2;
            var columnWidth = (outRect.width - 16f - (columns - 1) * gizmoSpacing) / columns;

            var rowCount = Mathf.CeilToInt((float)designators.Count / columns);
            var viewRect = new Rect(0, 0, outRect.width - 16f, rowCount * rowHeight);

            Designator mouseoverGizmo = null;
            Designator interactedGizmo = null;
            Designator floatMenuGizmo = null;
            Event interactedEvent = null;
            HandleScrollBar(outRect, viewRect, ref ordersScrollPosition);
            Widgets.BeginScrollView(outRect, ref ordersScrollPosition, viewRect);
            GizmoGridDrawer.drawnHotKeys.Clear();
            
            for (var i = 0; i < designators.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                var xPos = col * (gizmoSize + gizmoSpacing);
                var yPos = row * rowHeight;
                var designator = designators[i];
                var parms = new GizmoRenderParms
                {
                    highLight = ShouldHighLightGizmo(designator),
                    lowLight = ShouldLowLightGizmo(designator),
                    isFirst = i == 0,
                    multipleSelected = false
                };

                var result = designator.GizmoOnGUI(new Vector2(xPos, yPos), gizmoSize, parms);
                ProcessGizmoResult(result, designator, ref mouseoverGizmo, ref interactedGizmo, ref floatMenuGizmo, ref interactedEvent);
            }
            ProcessGizmoInteractions(interactedGizmo, floatMenuGizmo, interactedEvent);
            
            Widgets.EndScrollView();
            return mouseoverGizmo;
        }

        private static void DoInfoBox(Designator designator)
        {
            Find.WindowStack.ImmediateWindow(32520, ArchitectCategoryTab.InfoRect, WindowLayer.GameUI, delegate
            {
                if (designator == null) return;
                Rect rect = ArchitectCategoryTab.InfoRect.AtZero().ContractedBy(7f);
                Widgets.BeginGroup(rect);
                Rect titleRect = new Rect(0f, 0f, rect.width - designator.PanelReadoutTitleExtraRightMargin, 999f);
                Text.Font = GameFont.Small;
                Widgets.Label(titleRect, designator.LabelCap);
                float curY = Mathf.Max(24f, Text.CalcHeight(designator.LabelCap, titleRect.width));
                designator.DrawPanelReadout(ref curY, rect.width);
                Rect descRect = new Rect(0f, curY, rect.width, rect.height - curY);
                string desc = designator.Desc;
                GenText.SetTextSizeToFit(desc, descRect);
                desc = desc.TruncateHeight(descRect.width, descRect.height);
                Widgets.Label(descRect, desc);
                Widgets.EndGroup();
            });
        }

        private static bool MatchesSearch(Designator designator)
        {
            if (currentArchitectCategoryTab?.quickSearchFilter?.Active != true)
                return true;

            return currentArchitectCategoryTab.quickSearchFilter.Matches(designator.LabelCap);
        }

        private static bool ShouldLowLightGizmo(Designator designator)
        {
            if (currentArchitectCategoryTab?.quickSearchFilter?.Active != true)
                return false;

            return !MatchesSearch(designator);
        }

        private static bool ShouldHighLightGizmo(Designator designator)
        {
            if (currentArchitectCategoryTab?.quickSearchFilter?.Active != true)
                return false;

            return MatchesSearch(designator);
        }

        private static void ProcessGizmoResult(GizmoResult result, Designator designator, ref Designator mouseoverGizmo, ref Designator interactedGizmo, ref Designator floatMenuGizmo, ref Event interactedEvent)
        {
            if (result.State >= GizmoState.Mouseover) mouseoverGizmo = designator;
            if (result.State == GizmoState.Interacted)
            {
                interactedGizmo = designator;
                interactedEvent = result.InteractEvent;
            }
            else if (result.State == GizmoState.OpenedFloatMenu)
            {
                floatMenuGizmo = designator;
                interactedEvent = result.InteractEvent;
            }
        }
        
        private static void ProcessGizmoInteractions(Designator interactedGizmo, Designator floatMenuGizmo, Event interactedEvent)
        {
            if (interactedGizmo != null)
            {
                interactedGizmo.ProcessInput(interactedEvent);
                Event.current.Use();
            }
            if (floatMenuGizmo != null)
            {
                var floatMenuOptions = floatMenuGizmo.RightClickFloatMenuOptions.ToList();
                if (floatMenuOptions.Any())
                {
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    Event.current.Use();
                }
                else
                {
                    floatMenuGizmo.ProcessInput(interactedEvent);
                    Event.current.Use();
                }
            }
        }
        
        private static float GetCategoryViewHeight(int itemCount)
        {
            return itemCount * 41f;
        }

        private static void HandleScrollBar(Rect outRect, Rect viewRect, ref Vector2 scrollPosition)
        {
            if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect))
            {
                scrollPosition.y += Event.current.delta.y * 20f;
                float num = 0f;
                float num2 = viewRect.height - outRect.height;
                if (scrollPosition.y < num)
                {
                    scrollPosition.y = num;
                }
                if (scrollPosition.y > num2)
                {
                    scrollPosition.y = num2;
                }
                Event.current.Use();
            }
        }

        private static void DrawOptionBackground(Rect rect, bool selected, bool highlight = false, bool lowlight = false)
        {
            if (selected)
            {
                DrawOptionSelected(rect);
            }
            else
            {
                DrawOptionUnselected(rect, highlight, lowlight);
            }
            Widgets.DrawHighlightIfMouseover(rect);
        }

        public static void DrawOptionSelected(Rect rect)
        {
            GUI.color = Widgets.OptionSelectedBGFillColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Widgets.OptionSelectedBGBorderColor;
            Widgets.DrawBox(rect, 1);
            GUI.color = Color.white;
        }

        public static void DrawOptionUnselected(Rect rect, bool highlight = false, bool lowlight = false)
        {
            if (lowlight)
            {
                GUI.color = Color.grey;
            }
            else
            {
                GUI.color = Widgets.OptionUnselectedBGFillColor;
            }
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            if (!highlight && !lowlight)
            {
                GUI.color = Widgets.OptionUnselectedBGBorderColor;
                Widgets.DrawBox(rect, 1);
            }
            else if (highlight)
            {
                GUI.color = Color.yellow;
                Widgets.DrawBox(rect, 1);
            }
            GUI.color = Color.white;
        }
    }
}
