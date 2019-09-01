namespace FunWebService

module WebJobsStartup =
  open Microsoft.Azure.WebJobs
  open Microsoft.Azure.WebJobs.Hosting
  
  type WebJobsStartup() =
    interface IWebJobsStartup with
      member this.Configure _ = ()

  [<assembly: WebJobsStartup(typedefof<WebJobsStartup>)>]
  do ()  