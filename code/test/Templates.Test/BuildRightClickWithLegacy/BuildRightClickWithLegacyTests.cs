// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Extensions;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Helpers;
using Microsoft.Templates.Fakes;
using Xunit;

namespace Microsoft.Templates.Test
{
    [Collection("BuildRightClickWithLegacyCollection")]
    public class BuildRightClickWithLegacyTests : BaseGenAndBuildTests
    {
        private readonly string _emptyBackendFramework = string.Empty;
        //TODO: Remove once version 3.5 is released
        private string[] excludedTemplates = { "wts.Feat.MultiView", "wts.Page.TabView", "wts.Page.TabView.VB", "wts.Page.TreeView", "wts.Page.TreeView.VB" };

        public BuildRightClickWithLegacyTests(BuildRightClickWithLegacyFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [MemberData(nameof(BaseGenAndBuildTests.GetProjectTemplatesForBuild), "LegacyFrameworks")]
        [Trait("ExecutionSet", "BuildRightClickWithLegacy")]
        [Trait("Type", "BuildRightClickLegacy")]
        public async Task BuildEmptyLegacyProjectWithAllRightClickItemsAsync(string projectType, string framework, string platform, string language)
        {
            var fixture = _fixture as BuildRightClickWithLegacyFixture;

            if (language == ProgrammingLanguages.VisualBasic)
            {
                fixture.ChangeTemplatesSource(fixture.VBSource, language, Platforms.Uwp);
            }

            var projectName = $"{projectType}{framework}Legacy";

            var projectPath = await AssertGenerateProjectAsync(projectName, projectType, framework, Platforms.Uwp, language, null, null);

            fixture.ChangeToLocalTemplatesSource(fixture.LocalSource, language, Platforms.Uwp);

            var rightClickTemplates = _fixture.Templates().Where(
                t => t.GetTemplateType().IsItemTemplate()
                && t.GetFrontEndFrameworkList().Contains(framework)
                && !excludedTemplates.Contains(t.GroupIdentity)
                && t.GetPlatform() == platform
                && !t.GetIsHidden()
                && t.GetRightClickEnabled());

            await AddRightClickTemplatesAsync(GenContext.Current.DestinationPath, rightClickTemplates, projectName, projectType, framework, platform, language);

            AssertBuildProjectAsync(projectPath, projectName, platform);
        }

        [Theory]
        [MemberData(nameof(BaseGenAndBuildTests.GetProjectTemplatesForBuild), "LegacyFrameworks")]
        [Trait("ExecutionSet", "ManualOnly")]
        ////This test sets up projects for further manual tests. It generates legacy projects with all pages and features.
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public async Task GenerateLegacyProjectWithAllPagesAndFeaturesAsync(string projectType, string framework, string platform, string language)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            var fixture = _fixture as BuildRightClickWithLegacyFixture;

            if (language == ProgrammingLanguages.VisualBasic)
            {
                fixture.ChangeTemplatesSource(fixture.VBSource, language, Platforms.Uwp);
            }

            var projectName = $"{ProgrammingLanguages.GetShortProgrammingLanguage(language)}{ShortProjectType(projectType)}{framework}AllLegacy";

            Func<ITemplateInfo, bool> templateSelector =
                t => t.GetTemplateType().IsItemTemplate()
                && (t.GetProjectTypeList().Contains(projectType) || t.GetProjectTypeList().Contains(All))
                && t.GetFrontEndFrameworkList().Contains(framework)
                && t.GetPlatform() == platform
                && !t.GetIsHidden();

            var projectPath = await AssertGenerateProjectAsync(projectName, projectType, framework, platform, language, templateSelector, BaseGenAndBuildFixture.GetDefaultName);
        }
    }
}
