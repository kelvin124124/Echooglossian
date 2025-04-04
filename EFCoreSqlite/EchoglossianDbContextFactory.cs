// <copyright file="EchoglossianDbContextFactory.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using Microsoft.EntityFrameworkCore.Design;

namespace Echoglossian.EFCoreSqlite
{
    public class EchoglossianDbContextFactory : IDesignTimeDbContextFactory<EchoglossianDbContext>
    {
        public EchoglossianDbContext CreateDbContext(string[] args)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fullPath = Path.Combine(appDataPath, "XIVLauncher", "pluginConfigs", "Echoglossian");

            var configDir = fullPath + Path.DirectorySeparatorChar; /*Echoglossian.PluginInterface.GetPluginConfigDirectory() + Path.DirectorySeparatorChar;*/
            return new EchoglossianDbContext(configDir);
        }
    }
}