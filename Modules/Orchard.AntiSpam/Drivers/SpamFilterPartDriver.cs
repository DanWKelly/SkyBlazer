﻿using System;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.AntiSpam.Drivers {
    public class SpamFilterPartDriver : ContentPartDriver<SpamFilterPart> {
        private readonly ISpamService _spamService;
        private const string TemplateName = "Parts/SpamFilter";

        public SpamFilterPartDriver(IOrchardServices services, ISpamService  spamService) {
            _spamService = spamService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "SpamFilter"; }
        }

        protected override DriverResult Editor(SpamFilterPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper) {
            part.Status = _spamService.CheckForSpam(part);

            if (part.Settings.GetModel<SpamFilterPartSettings>().DeleteSpam) {
                updater.AddModelError("Spam", T("Spam detected."));
            }

            return Editor(part, shapeHelper);
        }

        protected override DriverResult Display(SpamFilterPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_SpamFilter_Metadata_SummaryAdmin", () => shapeHelper.Parts_SpamFilter_Metadata_SummaryAdmin()),
                ContentShape("Parts_SpamFilter_Metadata_Actions", () => shapeHelper.Parts_SpamFilter_Metadata_Actions())
                );
        }

        protected override void Importing(SpamFilterPart part, ImportContentContext context) {
            var status = context.Attribute(part.PartDefinition.Name, "Status");
            
            if (status != null) {
                SpamStatus value;
                if(Enum.TryParse(status, out value)) {
                    part.Status = value;
                }
            }
        }

        protected override void Exporting(SpamFilterPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Status", part.Status.ToString());
        }
    }
}