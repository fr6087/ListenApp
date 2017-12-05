using System.Collections.Generic;
using System.Threading.Tasks;

namespace ListenToMe
{
    internal interface IModelMethods
    {
        Task<List<string>> UpdatePhraseList(string phraseListName);
    }
}