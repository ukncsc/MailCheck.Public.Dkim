﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailCheck.Dkim.Entity.Dao {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DkimEntityDaoResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DkimEntityDaoResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MailCheck.Dkim.Entity.Dao.DkimEntityDaoResources", typeof(DkimEntityDaoResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM dkim_entity
        ///WHERE LOWER(id)= LOWER(@domain).
        /// </summary>
        internal static string DeleteDkimEntity {
            get {
                return ResourceManager.GetString("DeleteDkimEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO `dkim_entity`
        ///(`id`,
        ///`version`,
        ///`state`)
        ///VALUES
        ///(LOWER(@domain),
        ///@version,
        ///@state)
        ///ON DUPLICATE KEY UPDATE 
        ///state = IF(version&lt; @version, VALUES(state), state),
        ///version = IF(version &lt; @version, VALUES(version), version);
        ///.
        /// </summary>
        internal static string InsertDkimEntity {
            get {
                return ResourceManager.GetString("InsertDkimEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT state 
        ///FROM dkim_entity
        ///WHERE id = @domain
        ///ORDER BY version DESC
        ///LIMIT 1;.
        /// </summary>
        internal static string SelectDkimEntity {
            get {
                return ResourceManager.GetString("SelectDkimEntity", resourceCulture);
            }
        }
    }
}
