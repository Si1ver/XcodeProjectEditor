namespace UnityEditor.XCodeEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public partial class XCProject : System.IDisposable
    {
        private PBXDictionary _datastore;
        public PBXDictionary _objects;
        private PBXDictionary _configurations;

        private PBXGroup _rootGroup;
        private string _defaultConfigurationName;
        private string _rootObjectKey;

        public string projectRootPath { get; private set; }

        public string filePath { get; private set; }

        private string sourcePathRoot;
        private bool modified = false;

        // Objects
        private PBXDictionary<PBXBuildFile> _buildFiles;
        private PBXDictionary<PBXGroup> _groups;
        private PBXDictionary<PBXFileReference> _fileReferences;
        private PBXDictionary<PBXNativeTarget> _nativeTargets;

        private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
        private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
        private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
        private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
        private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;

        private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
        private PBXDictionary<XCConfigurationList> _configurationLists;

        private PBXProject _project;

        public XCProject()
        {
        }

        public XCProject(string filePath)
        {
            DiscoverXcodeProject(filePath);

            Debug.LogFormat("Opening project \"{0}\".", this.filePath);

            FileInfo projectFileInfo = new FileInfo(Path.Combine(this.filePath, "project.pbxproj"));

            string contents;
            using (StreamReader streamReader = projectFileInfo.OpenText())
            {
                contents = streamReader.ReadToEnd();
            }

            PBXParser parser = new PBXParser();
            _datastore = parser.Decode( contents );
            if( _datastore == null ) {
                throw new System.Exception( "Project file not found at file path " + this.filePath);
            }

            if( !_datastore.ContainsKey( "objects" ) ) {
                Debug.Log( "Errore " + _datastore.Count );
                return;
            }

            _objects = (PBXDictionary)_datastore["objects"];
            modified = false;

            _rootObjectKey = (string)_datastore["rootObject"];
            if( !string.IsNullOrEmpty( _rootObjectKey ) ) {
//              _rootObject = (PBXDictionary)_objects[ _rootObjectKey ];
                _project = new PBXProject( _rootObjectKey, (PBXDictionary)_objects[ _rootObjectKey ] );
//              _rootGroup = (PBXDictionary)_objects[ (string)_rootObject[ "mainGroup" ] ];
                _rootGroup = new PBXGroup( _rootObjectKey, (PBXDictionary)_objects[ _project.mainGroupID ] );
            }
            else {
                Debug.LogWarning( "error: project has no root object" );
                _project = null;
                _rootGroup = null;
            }
        }

        public PBXProject project {
            get {
                return _project;
            }
        }

        public PBXGroup rootGroup {
            get {
                return _rootGroup;
            }
        }

        public PBXDictionary<PBXBuildFile> buildFiles {
            get {
                if( _buildFiles == null ) {
                    _buildFiles = new PBXDictionary<PBXBuildFile>( _objects );
                }

                return _buildFiles;
            }
        }

        public PBXDictionary<PBXGroup> groups {
            get {
                if( _groups == null ) {
                    _groups = new PBXDictionary<PBXGroup>( _objects );
                }

                return _groups;
            }
        }

        public PBXDictionary<PBXFileReference> fileReferences {
            get {
                if( _fileReferences == null ) {
                    _fileReferences = new PBXDictionary<PBXFileReference>( _objects );
                }

                return _fileReferences;
            }
        }

        public PBXDictionary<PBXNativeTarget> nativeTargets {
            get {
                if( _nativeTargets == null ) {
                    _nativeTargets = new PBXDictionary<PBXNativeTarget>( _objects );
                }

                return _nativeTargets;
            }
        }

        public PBXDictionary<XCBuildConfiguration> buildConfigurations {
            get {
                if( _buildConfigurations == null ) {
                    _buildConfigurations = new PBXDictionary<XCBuildConfiguration>( _objects );
                }

                return _buildConfigurations;
            }
        }

        public PBXDictionary<XCConfigurationList> configurationLists {
            get {
                if( _configurationLists == null ) {
                    _configurationLists = new PBXDictionary<XCConfigurationList>( _objects );
                }

                return _configurationLists;
            }
        }

        public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases {
            get {
                if( _frameworkBuildPhases == null ) {
                    _frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>( _objects );
                }

                return _frameworkBuildPhases;
            }
        }

        public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases {
            get {
                if( _resourcesBuildPhases == null ) {
                    _resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>( _objects );
                }

                return _resourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases {
            get {
                if( _shellScriptBuildPhases == null ) {
                    _shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>( _objects );
                }

                return _shellScriptBuildPhases;
            }
        }

        public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases {
            get {
                if( _sourcesBuildPhases == null ) {
                    _sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>( _objects );
                }

                return _sourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases {
            get {
                if( _copyBuildPhases == null ) {
                    _copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>( _objects );
                }

                return _copyBuildPhases;
            }
        }

        public bool AddOtherCFlags( string flag )
        {
            return AddOtherCFlags( new PBXList( flag ) );
        }

        public bool AddOtherCFlags( PBXList flags )
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.AddOtherCFlags( flags );
            }

            modified = true;
            return modified;
        }

        public bool AddOtherLDFlags( string flag )
        {
            return AddOtherLDFlags( new PBXList( flag ) );
        }

        public bool AddOtherLDFlags( PBXList flags )
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.AddOtherLDFlags( flags );
            }

            modified = true;
            return modified;
        }

        public bool GccEnableCppExceptions (string value)
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.GccEnableCppExceptions( value );
            }

            modified = true;
            return modified;
        }

        public bool GccEnableObjCExceptions (string value)
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.GccEnableObjCExceptions( value );
            }

            modified = true;
            return modified;
        }

        public bool AddHeaderSearchPaths( string path )
        {
            return AddHeaderSearchPaths( new PBXList( path ) );
        }

        public bool AddHeaderSearchPaths( PBXList paths )
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.AddHeaderSearchPaths( paths );
            }

            modified = true;
            return modified;
        }

        public bool AddLibrarySearchPaths( string path )
        {
            return AddLibrarySearchPaths( new PBXList( path ) );
        }

        public bool AddLibrarySearchPaths( PBXList paths )
        {
            foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
                buildConfig.Value.AddLibrarySearchPaths( paths );
            }

            modified = true;
            return modified;
        }

        public bool AddFrameworkSearchPaths(string path)
        {
            return AddFrameworkSearchPaths(new PBXList(path));
        }

        public bool AddFrameworkSearchPaths(PBXList paths)
        {
            foreach (KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddFrameworkSearchPaths(paths);
            }

            modified = true;
            return modified;
        }

        //FRAMEWORK_SEARCH_PATHS = (
            //      "$(inherited)",
                //  "\"$(SRCROOT)/../../../../../../../Documents/FacebookSDK\"",
                //);

//      public PBXList GetObjectOfType( string type )
//      {
//          PBXList result = new PBXList();
//          foreach( KeyValuePair<string, object> current in _objects ) {
//              if( string.Compare( (string)((PBXDictionary)current.Value)["isa"], type ) == 0 )
//                  result.Add( current.Value );
//          }
//          return result;
//      }

        public object GetObject( string guid )
        {
            return _objects[guid];
        }

//      public PBXDictionary<PBXBuildPhase> GetBuildPhase( string buildPhase )
//      {
//          switch( buildPhase ) {
//              case "PBXFrameworksBuildPhase":
//                  return (PBXDictionary<PBXBuildPhase>)frameworkBuildPhases;
//              case "PBXResourcesBuildPhase":
//                  return (PBXDictionary<PBXBuildPhase>)resourcesBuildPhases;
//              case "PBXShellScriptBuildPhase":
//                  return (PBXDictionary<PBXBuildPhase>)shellScriptBuildPhases;
//              case "PBXSourcesBuildPhase":
//                  return (PBXDictionary<PBXBuildPhase>)sourcesBuildPhases;
//              case "PBXCopyFilesBuildPhase":
//                  return (PBXDictionary<PBXBuildPhase>)copyBuildPhases;
//              default:
//                  return default(T);
//          }
//      }

        public PBXDictionary AddFile( string filePath, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false )
        {
            PBXDictionary results = new PBXDictionary();
            string absPath = string.Empty;

            if( Path.IsPathRooted( filePath ) ) {
                absPath = filePath;
            }
            else if( tree.CompareTo( "SDKROOT" ) != 0) {
                absPath = Path.Combine( Application.dataPath.Replace("Assets", ""), filePath );
            }

            if( !( File.Exists( absPath ) || Directory.Exists( absPath ) ) && tree.CompareTo( "SDKROOT" ) != 0 ) {
                Debug.Log( "Missing file: " + absPath + " > " + filePath );
                return results;
            }
            else if( tree.CompareTo( "SOURCE_ROOT" ) == 0 || tree.CompareTo( "GROUP" ) == 0 ) {
                System.Uri fileURI = new System.Uri( absPath );
                System.Uri rootURI = new System.Uri( ( projectRootPath + "/." ) );
                filePath = rootURI.MakeRelativeUri( fileURI ).ToString();
            }

            if( parent == null ) {
                parent = _rootGroup;
            }

            // TODO: Aggiungere controllo se file già presente
            PBXFileReference fileReference = GetFile( System.IO.Path.GetFileName( filePath ) );
            if( fileReference != null ) {
                return null;
            }

            fileReference = new PBXFileReference( filePath, (TreeEnum)System.Enum.Parse( typeof(TreeEnum), tree ) );
            parent.AddChild( fileReference );
            fileReferences.Add( fileReference );
            results.Add( fileReference.guid, fileReference );

            //Create a build file for reference
            if( !string.IsNullOrEmpty( fileReference.buildPhase ) && createBuildFiles ) {
//              PBXDictionary<PBXBuildPhase> currentPhase = GetBuildPhase( fileReference.buildPhase );
                PBXBuildFile buildFile;
                switch( fileReference.buildPhase ) {
                    case "PBXFrameworksBuildPhase":
                        foreach( KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases ) {
                            buildFile = new PBXBuildFile( fileReference, weak );
                            buildFiles.Add( buildFile );
                            currentObject.Value.AddBuildFile( buildFile );
                        }

                        if ( !string.IsNullOrEmpty( absPath ) && File.Exists(absPath) && tree.CompareTo( "SOURCE_ROOT" ) == 0 ) {
                            string libraryPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
                            this.AddLibrarySearchPaths( new PBXList(libraryPath) );
                        }
                        else if (!string.IsNullOrEmpty( absPath ) && Directory.Exists(absPath) && absPath.EndsWith(".framework") && tree.CompareTo("GROUP") == 0) { // Annt: Add framework search path for FacebookSDK
                            string frameworkPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
                            this.AddFrameworkSearchPaths(new PBXList(frameworkPath));
                        }

                        break;
                    case "PBXResourcesBuildPhase":
                        foreach( KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases ) {
                            buildFile = new PBXBuildFile( fileReference, weak );
                            buildFiles.Add( buildFile );
                            currentObject.Value.AddBuildFile( buildFile );
                        }

                        break;
                    case "PBXShellScriptBuildPhase":
                        foreach( KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases ) {
                            buildFile = new PBXBuildFile( fileReference, weak );
                            buildFiles.Add( buildFile );
                            currentObject.Value.AddBuildFile( buildFile );
                        }

                        break;
                    case "PBXSourcesBuildPhase":
                        foreach( KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases ) {
                            buildFile = new PBXBuildFile( fileReference, weak );
                            buildFiles.Add( buildFile );
                            currentObject.Value.AddBuildFile( buildFile );
                        }

                        break;
                    case "PBXCopyFilesBuildPhase":
                        foreach( KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases ) {
                            buildFile = new PBXBuildFile( fileReference, weak );
                            buildFiles.Add( buildFile );
                            currentObject.Value.AddBuildFile( buildFile );
                        }

                        break;
                    case null:
                        Debug.LogWarning( "fase non supportata null" );
                        break;
                    default:
                        Debug.LogWarning( "fase non supportata def" );
                        return null;
                }
            }

            return results;
        }

        public bool AddFolder( string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true )
        {
            if( !Directory.Exists( folderPath ) )
                return false;
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo( folderPath );

            if( exclude == null )
                exclude = new string[] {};
            string regexExclude = string.Format( @"{0}", string.Join( "|", exclude ) );

            if( parent == null )
                parent = rootGroup;

            // Create group
            PBXGroup newGroup = GetGroup( sourceDirectoryInfo.Name, null /*relative path*/, parent );
//          groups.Add( newGroup );

            foreach( string directory in Directory.GetDirectories( folderPath ) )
            {
                if( Regex.IsMatch( directory, regexExclude ) ) {
                    continue;
                }

//              special_folders = ['.bundle', '.framework', '.xcodeproj']
                Debug.Log( "DIR: " + directory );
                if( directory.EndsWith( ".bundle" ) ) {
                    // Treath it like a file and copy even if not recursive
                    Debug.LogWarning( "This is a special folder: " + directory );
                    AddFile( directory, newGroup, "SOURCE_ROOT", createBuildFile );
                    Debug.Log( "fatto" );
                    continue;
                }

                if( recursive ) {
                    Debug.Log( "recursive" );
                    AddFolder( directory, newGroup, exclude, recursive, createBuildFile );
                }
            }

            // Adding files.
            foreach( string file in Directory.GetFiles( folderPath ) ) {
                if( Regex.IsMatch( file, regexExclude ) ) {
                    continue;
                }

                AddFile( file, newGroup, "SOURCE_ROOT", createBuildFile );
            }

            modified = true;
            return modified;
        }

        public PBXFileReference GetFile( string name )
        {
            if( string.IsNullOrEmpty( name ) ) {
                return null;
            }

            foreach( KeyValuePair<string, PBXFileReference> current in fileReferences ) {
                if( !string.IsNullOrEmpty( current.Value.name ) && current.Value.name.CompareTo( name ) == 0 ) {
                    return current.Value;
                }
            }

            return null;
        }

        public PBXGroup GetGroup( string name, string path = null, PBXGroup parent = null )
        {
            if( string.IsNullOrEmpty( name ) )
                return null;

            if( parent == null )
                parent = rootGroup;

            foreach( KeyValuePair<string, PBXGroup> current in groups ) {
                if( string.IsNullOrEmpty( current.Value.name ) ) {
                    if( current.Value.path.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
                        return current.Value;
                    }
                }
                else if( current.Value.name.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
                    return current.Value;
                }
            }

            PBXGroup result = new PBXGroup( name, path );
            groups.Add( result );
            parent.AddChild( result );

            modified = true;
            return result;
        }

//      /// <summary>
//      /// Returns all file resources in the project, as an array of `XCSourceFile` objects.
//      /// </summary>
//      /// <returns>
//      /// The files.
//      /// </returns>
//      public ArrayList GetFiles()
//      {
//          return null;
//      }
//
//      /// <summary>
//      /// Returns the project file with the specified key, or nil.
//      /// </summary>
//      /// <returns>
//      /// The file with key.
//      /// </returns>
//      /// <param name='key'>
//      /// Key.
//      /// </param>
//      public XCSourceFile GetFileWithKey( string key )
//      {
//          return null;
//      }
//
//      /// <summary>
//      /// Returns the project file with the specified name, or nil. If more than one project file matches the specified name,
//      /// which one is returned is undefined.
//      /// </summary>
//      /// <returns>
//      /// The file with name.
//      /// </returns>
//      /// <param name='name'>
//      /// Name.
//      /// </param>
//      public XCSourceFile GetFileWithName( string name )
//      {
//          return null;
//      }
//
//      /// <summary>
//      /// Returns all header files in the project, as an array of `XCSourceFile` objects.
//      /// </summary>
//      /// <returns>
//      /// The header files.
//      /// </returns>
//      public ArrayList GetHeaderFiles()
//      {
//          return null;
//      }
//
//      /**
//      * Returns all implementation obj-c implementation files in the project, as an array of `XCSourceFile` objects.
//      */
//      public ArrayList GetObjectiveCFiles()
//      {
//          return null;
//      }
//
//      /**
//      * Returns all implementation obj-c++ implementation files in the project, as an array of `XCSourceFile` objects.
//      */
//      public ArrayList GetObjectiveCPlusPlusFiles()
//      {
//          return null;
//      }
//
//      /**
//      * Returns all the xib files in the project, as an array of `XCSourceFile` objects.
//      */
//      public ArrayList GetXibFiles()
//      {
//          return null;
//
//      }
//
//      public ArrayList getImagePNGFiles()
//      {
//          return null;
//      }
//
//      /**
//      * Lists the groups in an xcode project, returning an array of `PBXGroup` objects.
//      */
//      public PBXList groups {
//          get {
//              return null;
//          }
//      }
//
//      /**
//       * Returns the root (top-level) group.
//       */
//      public PBXGroup rootGroup {
//          get {
//              return null;
//          }
//      }
//
//      /**
//       * Returns the root (top-level) groups, if there are multiple. An array of rootGroup if there is only one.
//       */
//      public ArrayList rootGroups {
//          get {
//              return null;
//          }
//      }
//
//      /**
//      * Returns the group with the given key, or nil.
//      */
//      public PBXGroup GetGroupWithKey( string key )
//      {
//          return null;
//      }
//
//      /**
//       * Returns the group with the specified display name path - the directory relative to the root group. Eg Source/Main
//       */
//      public PBXGroup GetGroupWithPathFromRoot( string path )
//      {
//          return null;
//      }
//
//      /**
//      * Returns the parent group for the group or file with the given key;
//      */
//      public PBXGroup GetGroupForGroupMemberWithKey( string key )
//      {
//          return null;
//      }
//
//      /**
//       * Returns the parent group for the group or file with the source file
//       */
//      public PBXGroup GetGroupWithSourceFile( XCSourceFile sourceFile )
//      {
//          return null;
//      }
//
//      /**
//      * Lists the targets in an xcode project, returning an array of `XCTarget` objects.
//      */
//      public ArrayList targets {
//          get {
//              return null;
//          }
//      }
//
//      /**
//      * Returns the target with the specified name, or nil.
//      */
//      public XCTarget GetTargetWithName( string name )
//      {
//          return null;
//      }
//
//      /**
//      * Returns the target with the specified name, or nil.
//      */
//      public Dictionary<string, string> configurations {
//          get {
//              return null;
//          }
//      }
//
//      public Dictionary<string, string> GetConfigurationWithName( string name )
//      {
//          return null;
//      }
//
//      public XCBuildConfigurationList defaultConfiguration {
//          get {
//              return null;
//          }
//      }

        public void ApplyMod( string pbxmod )
        {
            XCMod mod = new XCMod( pbxmod );
            ApplyMod( mod );
        }

        public void ApplyMod( XCMod mod )
        {
            PBXGroup modGroup = this.GetGroup( mod.group );

            Debug.Log( "Adding libraries..." );
            PBXGroup librariesGroup = this.GetGroup( "Libraries" );
            foreach( XCModFile libRef in mod.libs ) {
                string completeLibPath = System.IO.Path.Combine( "usr/lib", libRef.filePath );
                this.AddFile( completeLibPath, modGroup, "SDKROOT", true, libRef.isWeak );
            }

            Debug.Log( "Adding frameworks..." );
            PBXGroup frameworkGroup = this.GetGroup( "Frameworks" );
            foreach( string framework in mod.frameworks ) {
                string[] filename = framework.Split( ':' );
                bool isWeak = ( filename.Length > 1 ) ? true : false;
                string completePath = System.IO.Path.Combine( "System/Library/Frameworks", filename[0] );
                this.AddFile( completePath, frameworkGroup, "SDKROOT", true, isWeak );
            }

            Debug.Log( "Adding files..." );
            foreach( string filePath in mod.files ) {
                string absoluteFilePath = System.IO.Path.Combine( mod.path, filePath );

                if( filePath.EndsWith(".framework") )
                    this.AddFile( absoluteFilePath, frameworkGroup, "GROUP", true, false);
                else
                    this.AddFile( absoluteFilePath, modGroup );
            }

            Debug.Log( "Adding folders..." );
            foreach( string folderPath in mod.folders ) {
                string absoluteFolderPath = System.IO.Path.Combine( mod.path, folderPath );
                this.AddFolder( absoluteFolderPath, modGroup, (string[])mod.excludes.ToArray( typeof(string) ) );
            }

            Debug.Log( "Adding headerpaths..." );
            foreach( string headerpath in mod.headerpaths ) {
                string absoluteHeaderPath = System.IO.Path.Combine( mod.path, headerpath );
                this.AddHeaderSearchPaths( absoluteHeaderPath );
            }

            Debug.Log( "Configure build settings..." );
            Hashtable buildSettings = mod.buildSettings;
            if (mod.buildSettings != null)
            {
                if( buildSettings.ContainsKey("OTHER_LDFLAGS") )
                {
                    Debug.Log( "    Adding other linker flags..." );
                    ArrayList otherLinkerFlags = (ArrayList) buildSettings["OTHER_LDFLAGS"];
                    foreach( string linker in otherLinkerFlags )
                    {
                        string _linker = linker;
                        if( !_linker.StartsWith("-") )
                            _linker = "-" + _linker;
                        this.AddOtherLDFlags( _linker );
                    }
                }

                if( buildSettings.ContainsKey("GCC_ENABLE_CPP_EXCEPTIONS") )
                {
                    Debug.Log( "    GCC_ENABLE_CPP_EXCEPTIONS = " + buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"] );
                    this.GccEnableCppExceptions( (string) buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"] );
                }

                if( buildSettings.ContainsKey("GCC_ENABLE_OBJC_EXCEPTIONS") )
                {
                    Debug.Log( "    GCC_ENABLE_OBJC_EXCEPTIONS = " + buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"] );
                    this.GccEnableObjCExceptions( (string) buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"] );
                }
            }

            this.Consolidate();
        }

        public void Consolidate()
        {
            PBXDictionary consolidated = new PBXDictionary();
            consolidated.Append<PBXBuildFile>( this.buildFiles );
            consolidated.Append<PBXGroup>( this.groups );
            consolidated.Append<PBXFileReference>( this.fileReferences );
//          consolidated.Append<PBXProject>( this.project );
            consolidated.Append<PBXNativeTarget>( this.nativeTargets );
            consolidated.Append<PBXFrameworksBuildPhase>( this.frameworkBuildPhases );
            consolidated.Append<PBXResourcesBuildPhase>( this.resourcesBuildPhases );
            consolidated.Append<PBXShellScriptBuildPhase>( this.shellScriptBuildPhases );
            consolidated.Append<PBXSourcesBuildPhase>( this.sourcesBuildPhases );
            consolidated.Append<PBXCopyFilesBuildPhase>( this.copyBuildPhases );
            consolidated.Append<XCBuildConfiguration>( this.buildConfigurations );
            consolidated.Append<XCConfigurationList>( this.configurationLists );
            consolidated.Add( project.guid, project.data );
            _objects = consolidated;
            consolidated = null;
        }

        public void Backup()
        {
            string backupPath = Path.Combine( this.filePath, "project.backup.pbxproj" );

            // Delete previous backup file
            if( File.Exists( backupPath ) )
                File.Delete( backupPath );

            // Backup original pbxproj file first
            File.Copy( System.IO.Path.Combine( this.filePath, "project.pbxproj" ), backupPath );
        }

        /// <summary>
        /// Saves a project after editing.
        /// </summary>
        public void Save()
        {
            PBXDictionary result = new PBXDictionary();
            result.Add( "archiveVersion", 1 );
            result.Add( "classes", new PBXDictionary() );
            result.Add( "objectVersion", 45 );

            Consolidate();
            result.Add( "objects", _objects );

            result.Add( "rootObject", _rootObjectKey );

            Backup();

            // Parse result object directly into file
            PBXParser parser = new PBXParser();
            StreamWriter saveFile = File.CreateText( System.IO.Path.Combine( this.filePath, "project.pbxproj" ) );
            saveFile.Write( parser.Encode( result, false ) );
            saveFile.Close();
        }

        /**
        * Raw project data.
        */
        public Dictionary<string, object> objects {
            get {
                return null;
            }
        }

        public void Dispose()
        {
        }

        private void DiscoverXcodeProject(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(
                    "Path to project can not be empty or null.", "filePath");
            }

            // Note: this will throw exception if filePath is null or empty.
            string fullFilePath = Path.GetFullPath(filePath);

            // Ensure that there is no trailing slash in directory name.
            char lastCharacter = fullFilePath[fullFilePath.Length - 1];
            if (lastCharacter == '\\' || lastCharacter == '/')
            {
                fullFilePath = fullFilePath.Substring(0, fullFilePath.Length - 1);
            }

            if (!Directory.Exists(fullFilePath))
            {
                throw new ArgumentException(
                    string.Format("Path \"{0}\" does not exists.", fullFilePath), "filePath");
            }

            if (fullFilePath.EndsWith(".xcodeproj"))
            {
                this.projectRootPath = Path.GetDirectoryName(fullFilePath);
                this.filePath = fullFilePath;
            }
            else
            {
                Debug.LogFormat("Looking for xcodeproj files in \"{0}\".", fullFilePath);

                string[] projects = Directory.GetDirectories(fullFilePath, "*.xcodeproj");
                if (projects.Length == 0)
                {
                    string errorText = string.Format(
                        "Error: Xcode project not found in directory \"{0}\".", fullFilePath);

                    throw new ArgumentException(errorText, "filePath");
                }
                else if (projects.Length > 1)
                {
                    Debug.LogWarningFormat(
                        "Warning: multiple Xcode projects found in directory \"{0}\".",
                        fullFilePath);
                }

                this.projectRootPath = fullFilePath;
                this.filePath = projects[0];
            }
        }
    }
}
