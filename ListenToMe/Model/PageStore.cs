using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;

namespace ListenToMe.Model
{
    public sealed class PageStore
    {
        private bool loaded;

        /// <summary>
        /// Persist the loaded trips in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<myPage> pages;

        public PageStore()
        {
            loaded = false;
            Pages = new ObservableCollection<myPage>();
        }

        /// <summary>
        /// Persisted trips, reloaded across executions of the application
        /// </summary>
        public ObservableCollection<myPage> Pages
        {
            get
            {
                return pages;
            }
            private set
            {
                pages = value;
            }
        }

        /// <summary>
        /// Load trips from a file on first-launch of the app. If the file does not yet exist,
        /// pre-seed it with several trips, in order to give the app demonstration data.
        /// </summary>
        public async Task LoadPages()
        {
            Debug.Write("LoadPages");
            // Ensure that we don't load trip data more than once.
            if (loaded)
            {
                return;
            }
            loaded = true;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.pages.Clear();

            var item = await folder.TryGetItemAsync("pages.xml");
            if (item == null)
            {
                /*// Add some 'starter' trips
                pages.Add(
                    new Trip()
                    {
                        Destination = "London",
                        Description = "Trip to London!",
                        StartDate = new DateTime(2015, 5, 5),
                        EndDate = new DateTime(2015, 5, 15)
                    });
                trips.Add(
                    new Trip()
                    {
                        Destination = "Melbourne",
                        Description = "Trip to Australia",
                        StartDate = new DateTime(2016, 2, 2),
                        EndDate = new DateTime(2016, 5, 17),
                        Notes = "Bring Sunscreen!"
                    });
                trips.Add(
                    new Trip()
                    {
                        Destination = "Yosemite National Park",
                        Description = "Trip to Yosemite",
                        StartDate = new DateTime(2015, 7, 11),
                        EndDate = new DateTime(2015, 7, 19),
                        Notes = "Buy some new hiking boots"
                    });
                await WriteTrips();*/
                return;
            }

            // Load pages out of a simple XML format. For the purposes of this example, we're treating
            // parse failures as "no trips exist" which will result in the file being erased.
            if (item.IsOfType(StorageItemTypes.File))
            {
                StorageFile pagesFile = item as StorageFile;

                string pageXmlText = await FileIO.ReadTextAsync(pagesFile);

                try
                {
                    XElement xmldoc = XElement.Parse(pageXmlText);

                    var pageElements = xmldoc.Descendants("Page");
                    foreach (var pageElement in pageElements)
                    {
                        myPage page = new myPage();

                        var destElement = pageElement.Descendants("Destination").FirstOrDefault();
                        if (destElement != null)
                        {
                            page.destination = destElement.Value;
                        }


                        var descElement = pageElement.Descendants("fields").FirstOrDefault().Descendants();
                        int iter = 0;
                        while (iter < descElement.Count())
                        {
                            String el = descElement.Elements().ElementAt(iter).Value;
                            if (el != null)
                            {
                                page.fields.Add(el);
                            }
                            iter++;
                        }


                        //toDo others
                        /*
                        var endElement = tripElement.Descendants("EndDate").FirstOrDefault();
                        if (endElement != null)
                        {
                            DateTime endDate;
                            if (DateTime.TryParse(endElement.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out endDate))
                            {
                                trip.EndDate = endDate;
                            }
                            else
                            {
                                trip.EndDate = null;
                            }
                        }

                        var notesElement = tripElement.Descendants("Notes").FirstOrDefault();
                        if (notesElement != null)
                        {
                            trip.Notes = notesElement.Value;
                        }*/

                        pages.Add(page);
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
        /// Delete a trip from the persistent trip store, and save the trips file.
        /// </summary>
        /// <param name="page">The trip to delete. If the trip is not an existing trip in the store,
        /// will not have an effect.</param>
        public async Task DeletePage(myPage page)
        {
            pages.Remove(page);
            await WritePages();
        }

        /// <summary>
        /// Add a trip to the persistent trip store, and saves the trips data file.
        /// </summary>
        /// <param name="page">The trip to save or update in the data file.</param>
        public async Task SavePage(myPage page)
        {
            if (!pages.Contains(page))
            {
                pages.Add(page);
            }

            await WritePages();
        }

        /// <summary>
        /// Write out a new XML file, overwriting the existing one if it already exists
        /// with the currently persisted trips. See class comment for basic format.
        /// </summary>
        private async Task WritePages()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            XElement xmldoc = new XElement("Root");
            XElement allFields = new XElement("fields");

            StorageFile pagesFile;

            var item = await folder.TryGetItemAsync("pages.xml");
            if (item == null)
            {
                pagesFile = await folder.CreateFileAsync("pages.xml");
            }
            else
            {
                pagesFile = await folder.GetFileAsync("pages.xml");
            }

            foreach (var page in Pages)
            {
                /*string startDateField = null;
                if (page.StartDate.HasValue)
                {
                    startDateField = page.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                string endDateField = null;
                if (page.EndDate.HasValue)
                {
                    endDateField = page.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }*/
                if (page.fields.Count > 0)
                {
                    for (int i = 0; i < page.fields.Count; i++)
                    {
                        if (!page.fields.ElementAt(i).Equals(null) && !page.fields.ElementAt(i).Equals(""))
                        {
                            allFields.Add(new XElement("field" + i, page.fields.ElementAt(i).ToString()));
                        }
                    }
                }

                if (page.switches.Count > 0)
                {
                    for (int i = 0; i < page.switches.Count; i++)
                    {
                        if (!page.switches.ElementAt(i).Equals(null))
                        {
                            allFields.Add(new XElement("switch" + i, page.switches.ElementAt(i).ToString()));
                        }
                    }
                }
                //todo others

                xmldoc.Add(
                    new XElement("Page",
                    new XElement("Destination", page.destination),
                    //new XElement("Notes", page.Notes
                    allFields
                    ));
            }

            await FileIO.WriteTextAsync(pagesFile, xmldoc.ToString());
        }
    }
}

