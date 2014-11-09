using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin(
	"EmacsKeys",
	Namespace = "Nsf",
	Version = "1.0",
	Category = "WTF"
)]

[assembly:AddinName("Emacs Keys")]
[assembly:AddinDescription("Adds additional emacs commands")]
[assembly:AddinAuthor("nsf <no.smile.face@gmail.com>")]

[assembly:AddinDependency("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
