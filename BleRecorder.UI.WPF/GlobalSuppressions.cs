// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;


// this suppresses warning about Win platform for WPF project 
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Single platform", Scope = "module")]
