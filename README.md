
<h3>Why would you customize a controller selector?</h3>


some developers use multiple controllers with the same name but that are in different namespaces. 

sometimes used it to create multiple areas belong controllers with the same name.

Changing the controller selector for the above scenarios works well for the purpose of selection Controller based on name and namespace.


<h4>So what to do?</h4>

All we have to do is implementing IHttpControllerSelector, let's see

<code>
<pre>
public interface IHttpControllerSelector
{
    HttpControllerDescriptor SelectController(HttpRequestMessage request);
    IDictionary<string, HttpControllerDescriptor> GetControllerMapping();
}
</pre>
</code>

Take a look at GetControllerMapping. The controller is mapped to a name without namespace or any more information. However, the ApiExplorer is not aware of controllers with the same name but in different namespaces.
Then we drive NamespaceHttpControllerSelector from IHttpControllerSelector and registering it in WebApiConfig (actually replace with default selector)

<h3>This way :</h3>

<code><b>config.Services.Replace(typeof(IHttpControllerSelector),new NamespaceHttpControllerSelector(config));<b></code>