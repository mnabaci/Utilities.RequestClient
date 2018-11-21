#   **RequestClient**
------------------------------

![alt tag](https://raw.githubusercontent.com/mnabaci/Utilities.RequestClient/master/misc/request-client-icon-with-text.png)

Easiest way to handle _rest requests_ 

[![Build status](https://ci.appveyor.com/api/projects/status/swmrnn5l4ru48lck?svg=true)](https://ci.appveyor.com/project/mnabaci/utilities-requestclient)
[![NuGet version](https://badge.fury.io/nu/Utilities.RequestClient.svg)](https://badge.fury.io/nu/Utilities.RequestClient)

### NuGet Packages
``` 
PM> Install-Package Utilities.RequestClient
```

#### Features:
- Currently supports XML, JSON and BSON request contents and **does not** support primitive types as return object excluding string
- Provides easy way to handle **GET**, **POST**, **PUT** and **DELETE** requests as sync or async.

Usage:
-----

Creating instance for **RequestClient**:

```cs
using (RequestClient requestClient = RequestClient.SetBaseUri(string baseUri))
{
}
```


after creating instance for _RequestClient_ then you can use **POST**, **GET**, **PUT** or **DELETE** methods for requests:

```cs
requestClient.Get<ResultDto>($"get-action?queryString=value");
requestClient.Post<ResultDto>("post-action", RequestDto requestDto);
requestClient.Put<ResultDto>("put-action", RequestDto requestDto);
requestClient.Delete<ResultDto>($"delete-action?queryString=value");
```

or you can use **Async** methods for requests:

```cs
await requestClient.GetAsync<ResultDto>($"get-action?queryString=value");
await requestClient.PostAsync<ResultDto>("post-action", RequestDto requestDto);
await requestClient.PutAsync<ResultDto>("put-action", RequestDto requestDto);
await requestClient.DeleteAsync<ResultDto>($"delete-action?queryString=value");
```

You can configure requests:

```cs
requestClient
    .AddHeader(string key,string value)
    .SetTimeout(TimeSpan.FromMilliseconds(10000))
    .Get<ResultDto>($"get-action?queryString=value");

```


There are several options you can set:

- `.AddHeader(string key, string value)`
- `.SetTimeout(TimeSpan timespan)` or `.SetTimeout(double milliseconds)`
- `.SetEncoding(Encoding encoding)`
- `.SetMediaType(MediaTypes mediaType)`
- `.SetAuthorizationHeader(string scheme, string parameter)`
- `.SetBasicAuthorizationHeader(string basicAuthHeader)`
- `.SetBearerAuthorizationHeader(string bearerAuthHeader)`
