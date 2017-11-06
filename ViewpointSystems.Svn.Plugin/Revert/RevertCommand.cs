﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using NationalInstruments;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.DataTypes;
using NationalInstruments.Design;
using NationalInstruments.MocCommon.Design;
using NationalInstruments.MocCommon.SourceModel;
using NationalInstruments.ProjectExplorer.Design;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.VI.SourceModel;
using NationalInstruments.SourceModel.Envoys;
using ViewpointSystems.Svn.Plugin.UserPreferences;
using System.ComponentModel.Composition;

namespace ViewpointSystems.Svn.Plugin.ReleaseLock
{
    [ExportPushCommandContent]
    public class RevertCommand : PushCommandContent
    {
        public static readonly ICommandEx RevertShellRelayCommand = new ShellRelayCommand(Revert)
        {
            UniqueId = "ViewpointSystems.Svn.Plugin.Revert.RevertShellRelayCommand",
            LabelTitle = "Revert",
        };

        [Import]
        public ICompositionHost Host { get; set; }

        /// <summary>
        /// Revert changes
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="host"></param>
        /// <param name="site"></param>
        public static void Revert(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var filePath = ((Envoy)parameter.Parameter).GetFilePath();
            var svnManager = host.GetSharedExportedValue<SvnManagerPlugin>();
            var success = svnManager.Revert(filePath);
            var debugHost = host.GetSharedExportedValue<IDebugHost>();
            if (success)
            {
                var envoy = ((Envoy)parameter.Parameter);
                var projectItem = envoy.GetProjectItemViewModel(site);
                if (null != projectItem)
                {
                    projectItem.RefreshIcon();
                }

                //This will revert a file once, but you have to close and reopen in order to revert the same file a second time.
                //If open, you also have to manually close the file and reopen to see the reversion.
                IReferencedFileService referencedFile = envoy.GetReferencedFileService();
                referencedFile.RefreshReferencedFileAsync();


                
                //var envoy = ((Envoy)parameter.Parameter);
                //ProjectItemViewModel projectItem = envoy.GetProjectItemViewModel(site);
                //if (null != projectItem)
                //{
                //    projectItem.RefreshIcon();
                //}
                debugHost.LogMessage(new DebugMessage("Viewpoint.Svn", DebugMessageSeverity.Information, $"Revert {filePath}"));
            }
            else
            {
                debugHost.LogMessage(new DebugMessage("Viewpoint.Svn", DebugMessageSeverity.Error, $"Failed to Revert {filePath}"));
            }

        }

        public override void CreateContextMenuContent(ICommandPresentationContext context, PlatformVisual sourceVisual)
        {
            var projectItem = sourceVisual.DataContext as ProjectItemViewModel;
            if (projectItem?.Envoy != null)
            {
                try
                {
                    var envoy = projectItem.Envoy;
                    if (envoy != null)
                    {
                        var svnManager = Host.GetSharedExportedValue<SvnManagerPlugin>();
                        var status = svnManager.Status(projectItem.FullPath);
                        if(status.IsVersioned && status.IsModified)                            
                            context.Add(new ShellCommandInstance(RevertShellRelayCommand) { CommandParameter = projectItem.Envoy });
                    }
                }
                catch (Exception)
                {
                }
            }
            base.CreateContextMenuContent(context, sourceVisual);
        }
    }
}