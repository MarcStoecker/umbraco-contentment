﻿/* Copyright © 2019 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Community.Contentment.DataEditors
{
    internal sealed class MacroPickerConfigurationEditor : ConfigurationEditor
    {
        private readonly IMacroService _macroService;

        internal const string AllowedMacros = "allowedMacros";
        internal const string AvailableMacros = "availableMacros";

        public MacroPickerConfigurationEditor(IMacroService macroService)
            : base()
        {
            _macroService = macroService;

            var macros = macroService
                .GetAll()
                .Select(x => new DataListItem
                {
                    Icon = Core.Constants.Icons.Macro,
                    Name = x.Name,
                    Description = x.Alias,
                    Value = x.GetUdi().ToString()
                });

            Fields.Add(
                AllowedMacros,
                "Allowed macros",
                "Restrict the macros that can be picked.",
                IOHelper.ResolveUrl(ItemPickerDataEditor.DataEditorViewPath),
                new Dictionary<string, object>
                {
                    { AllowDuplicatesConfigurationField.AllowDuplicates, Constants.Values.False },
                    { ItemPickerConfigurationEditor.Items, macros },
                    { ItemPickerTypeConfigurationField.ListType, ItemPickerTypeConfigurationField.List },
                    { ItemPickerConfigurationEditor.OverlayView, IOHelper.ResolveUrl(ItemPickerDataEditor.DataEditorOverlayViewPath) },
                    { EnableDevModeConfigurationField.EnableDevMode, Constants.Values.False },
                });

            // TODO: [LK:2019-09-25] Consider having a block-based UI option.

            Fields.Add(new MacroPickerDisplayModeConfigurationField());
            Fields.Add(new MaxItemsConfigurationField());
            Fields.Add(new DisableSortingConfigurationField());
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var config = base.ToValueEditor(configuration);

            if (config.TryGetValue(AllowedMacros, out var tmp1) && tmp1 is JArray array)
            {
                var ids = new List<Guid>();
                foreach (var token in array)
                {
                    if (GuidUdi.TryParse(token.Value<string>(), out var udi))
                    {
                        ids.Add(udi.Guid);
                    }
                }

                // TODO: [LK:2019-07-19] If this PR gets merged into Umbraco core, then uncomment the line below. https://github.com/umbraco/Umbraco-CMS/pull/5962
                //config.Add(AvailableMacros, _macroService.GetAll(ids).Select(x => x.Alias));
                config.Add(AvailableMacros, _macroService.GetAll().Where(x => ids.Contains(x.Key)).Select(x => x.Alias));
                config.Remove(AllowedMacros);
            }

            if (config.TryGetValue(MacroPickerDisplayModeConfigurationField.DisplayMode, out var tmp2) && tmp2 is string displayMode)
            {

            }

            // /App_Plugins/Contentment/editors/macro-picker-blocks.html

            return config;
        }

        internal class MacroPickerDisplayModeConfigurationField : ConfigurationField
        {
            public const string DisplayMode = "displayMode";
            public const string List = MacroPickerDataEditor.DataEditorListViewPath;
            public const string Blocks = MacroPickerDataEditor.DataEditorBlocksViewPath;

            public MacroPickerDisplayModeConfigurationField(string defaultValue = List)
                : base()
            {
                Key = DisplayMode;
                Name = "Display mode?";
                Description = "Select to display the macros in a list or as stacked blocks.";
                View = IOHelper.ResolveUrl(RadioButtonListDataEditor.DataEditorViewPath);
                Config = new Dictionary<string, object>
                {
                    { RadioButtonListConfigurationEditor.Items, new DataListItem[]
                        {
                            new DataListItem { Name = nameof(List), Value = List, Description = "This will display similar to a content picker." },
                            new DataListItem { Name = nameof(Blocks), Value = Blocks, Description = "This will display similar to UMCO's Stacked Content." },
                        }
                    },
                    { RadioButtonListConfigurationEditor.DefaultValue, defaultValue }
                };
            }
        }
    }
}
