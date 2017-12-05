using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace WcfService1
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IService1
    {
        /*These Operarions are for parsing the html-webform*/
        [OperationContract]
        void Login(String userName, String password);
        [OperationContract]
        string GetForm(String domainOfForm);
        [OperationContract]
        List<String> GetHeadings(String username, String password, String domain);
        [OperationContract]
        List<String> GetInputs(String username, String password, String domain);
        [OperationContract]
        String GetJason(String username, String password, String domain);


    }
}
