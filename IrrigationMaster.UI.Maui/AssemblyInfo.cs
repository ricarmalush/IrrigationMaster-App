using System.Runtime.CompilerServices;

// Permite que el proyecto de tests invoque directamente los métodos "internal"
// que respaldan los comandos de los ViewModels (ExecuteSave*Async), sin pasar
// por el fire-and-forget de ICommand.Execute.
[assembly: InternalsVisibleTo("IrrigationMaster.UI.Maui.Tests")]
