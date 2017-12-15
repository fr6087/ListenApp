using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;

namespace ClassLibrary.model
{
    public class FormStore
    {
        private bool loaded;

        /// <summary>
        /// Persist the loaded fields in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<Field> fields;

        public FormStore()
        {
            loaded = false;
            Fields = new ObservableCollection<Field>();
        }
        /// <summary>
        /// Persisted trips, reloaded across executions of the application
        /// </summary>
        public ObservableCollection<Field> Fields
        {
            get
            {
                return fields;
            }
            private set
            {
                fields = value;
            }
        }
        /// <summary>
        /// Load trips from a file on first-launch of the app. If the file does not yet exist,
        /// pre-seed it with several trips, in order to give the app demonstration data.
        /// </summary>
        public async Task LoadFields()
        {
            // Ensure that we don't load trip data more than once.
            if (loaded)
            {
                return;
            }
            loaded = true;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.fields.Clear();

            var item = await folder.TryGetItemAsync("fields.xml");
            if (item == null)
            {
                // Add some 'starter' trips
                fields.Add(
                    new Field()
                    {
                        Name = "LondonFieldName",
                        Text = "Field to London!"
                    });
                fields.Add(
                    new Field()
                    {
                        Name = "User Name",
                        Text = "Some fashinable user name",
                    });
                await WriteFields();
                return;
            }

            // Load trips out of a simple XML format. For the purposes of this example, we're treating
            // parse failures as "no trips exist" which will result in the file being erased.
            if (item.IsOfType(StorageItemTypes.File))
            {
                StorageFile tripsFile = item as StorageFile;

                string tripXmlText = await FileIO.ReadTextAsync(tripsFile);

                try
                {
                    XElement xmldoc = XElement.Parse(tripXmlText);

                    var fieldElements = xmldoc.Descendants("Field");
                    foreach (var fieldElement in fieldElements)
                    {
                        Field field = new Field();

                        var destElement = fieldElement.Descendants("Name").FirstOrDefault();
                        if (destElement != null)
                        {
                            field.Name = destElement.Value;
                        }

                        var descElement = fieldElement.Descendants("Text").FirstOrDefault();
                        if (descElement != null)
                        {
                            field.Text = descElement.Value;
                        }


                        Fields.Add(field);
                    }
                }
                catch (XmlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return;
                }

            }
        }
        /// <summary>
        /// Write out a new XML file, overwriting the existing one if it already exists
        /// with the currently persisted trips. See class comment for basic format.
        /// </summary>
        private async Task WriteFields()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            XElement xmldoc = new XElement("Root");

            StorageFile fieldsFile;

            var item = await folder.TryGetItemAsync("fields.xml");
            if (item == null)
            {
                fieldsFile = await folder.CreateFileAsync("fields.xml");
            }
            else
            {
                fieldsFile = await folder.GetFileAsync("fields.xml");
            }

            foreach (var field in Fields)
            {

                xmldoc.Add(
                    new XElement("Field",
                    new XElement("Name", field.Name),
                    new XElement("Text", field.Text)));
            }

            await FileIO.WriteTextAsync(fieldsFile, xmldoc.ToString());
        }


    }
}
