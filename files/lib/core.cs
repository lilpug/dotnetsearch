using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetSearchEngine
{
    public partial class SearchEngine
    {
        //This is the main function which triggers off the searching process and returns the results
        public DataTable Run()
        {
            //Checks the table and query exists before continueing otherwise just returns the same table back
            var temp = settings.table.Copy();
            if (temp != null && temp.Rows.Count > 0 && !string.IsNullOrWhiteSpace(settings.query))
            {
                //Checks if caching is enabled and if we can find any results for this query thats being asked for
                DataTable results = null;
                if(settings.isCacheEnabled)
                {
                    results = GetCacheResult(settings.query);
                }
                
                //Checks if we managed to find any cached results
                if (results == null)
                {
                    //Adds the weight column
                    temp.Columns.Add("dotnetsearch_search_weight", typeof(int));

                    //This processes the search results from the records passed
                    results = CoreSearch(temp);

                    //Checks if caching is enabled and if so adds the new results
                    //Note: we check the key in case they have run the clear cache, not reloaded the class before this function is hit
                    if (settings.isCacheEnabled && cachedTables.ContainsKey(settings.searchEngineName))
                    {
                        AddCacheResult(settings.query, results);
                    }
                }

                //This section deals with ordering the new results found
                
                //Determines the order and what should be output from the search results found
                var complete = ReturnResults(results);

                //Removes the weight column before returning
                complete.Columns.Remove("dotnetsearch_search_weight");
                
                //Removes the weight column as we no longer need it now
                return complete;                
            }
            //Return the original table back
            return temp;
        }

        //This functions deals with processing the records and creating the acceptable search records
        private DataTable CoreSearch(DataTable records)
        {
            //Splits the search up via spaces
            string[] searchTerms = settings.query.ToLower().Split(' ');
            
            //Holds all the records we deem accepted within this round of the search terms
            DataTable tempStorage = new DataTable();

            //Clones the records structure so they are the same for importing
            tempStorage = records.Clone();

            //loops over the different records via multithreading if cores specified
            Parallel.ForEach(records.AsEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = settings.multiThreadCores }, row =>
            //foreach (DataRow row in records.Rows)
            {
                //Runs the additional column check functions if any exist before continueing
                bool columnCheck = true;
                if (settings.extraVerificationChecks != null && settings.extraVerificationChecks.Count > 0)
                {
                    foreach (var function in settings.extraVerificationChecks)
                    {
                        if (!function(row))
                        {
                            columnCheck = false;
                            break;
                        }
                    }
                }

                //Check if all extra verifications went ok "if no extra verifications then it defaults to true"
                if (columnCheck)
                {
                    //These variables hold the weight of the search for that person
                    int weight = 0;
                    int previousWeight = 0;
                    bool ignoreStatus = false;

                    //Loops over all the search terms one by one to calculate the weight
                    foreach (string search in searchTerms)
                    {
                        //Checks if there is any additional weight checking functions and if so runs through them
                        if (settings.extraWeightChecks != null && settings.extraWeightChecks.Count > 0)
                        {
                            foreach (var function in settings.extraWeightChecks)
                            {
                                weight += function(row, search);
                            }
                        }

                        foreach (var col in records.Columns)
                        {
                            string columnName = col.ToString();

                            //Checks we are not suppose to ignore it
                            if (
                                //If only specific fields have been targeted then it ensures this is one of them otherwise it ignores it
                                (settings.onlyFieldsToCheck == null || settings.onlyFieldsToCheck.Contains(columnName)) &&

                                //Ensures the column is not within the ignore list
                                (settings.ignoreFields == null || !settings.ignoreFields.Contains(columnName)) &&

                                //Ensures the weight field is not in the search
                                columnName != "dotnetsearch_search_weight" &&

                                //Ensures there is a data value in the field
                                row[columnName] != null && row[columnName] != DBNull.Value
                                )
                            {
                                int matchNumber = Regex.Matches(Regex.Escape(row[columnName].ToString().ToLower()), search).Count;
                                if (matchNumber > 0)
                                {
                                    //Calculates the unique weight
                                    int uniqueWeight = 0;
                                    if (settings.weightings != null && settings.weightings.ContainsKey(columnName))
                                    {
                                        uniqueWeight = settings.weightings[columnName];
                                    }
                                    else
                                    {
                                        //Checks if defaultWeight is to be used otherwise its ignored and no weight is added as 0* something is 0
                                        uniqueWeight = (settings.allowDefault) ? defaultWeight : 0;
                                    }

                                    //Multiples the weight by the amount of occurances within the regex as it should be greater if appears multiple times
                                    weight += matchNumber * uniqueWeight;
                                }
                            }
                        }

                        //Adds the weight together with the previous weight to allow us to see if anythings changed
                        weight += previousWeight;

                        //Checks the weighting to see if we should keep it as a search record for anymore iterations or mark it as ignore
                        if (weight > 0 && weight > previousWeight)
                        {
                            //Sets the previous weight for the next loop iteration if any
                            previousWeight = weight;

                            //Resets the weight ready for any other iterations
                            weight = 0;
                        }
                        else
                        {
                            //Resets the weights
                            weight = 0;
                            previousWeight = 0;

                            //Flags that we should not be adding this record to the overall search results
                            ignoreStatus = true;

                            //Breaks out the word checks as we have already failed at this point
                            break;
                        }
                    }

                    //Checks if the ignore status is not active
                    if (!ignoreStatus)
                    {
                        //Puts the new weight against the record for that person
                        //Note: we need to lock this as we are writing to the datarow which is referenced from the single datatable 
                        //      although the datarow is singular its references from the datatable thus causing conflicts on a write in multi threads
                        lock (updateLocker)
                        {
                            row["dotnetsearch_search_weight"] = previousWeight;
                        }

                        //Locks the thread while we add the row as writing operations are not threadsafe
                        //Note: read operations are threadsafe.
                        lock (addLocker)
                        {
                            //Adds the record
                            tempStorage.ImportRow(row);
                        }
                    }
                }
             });       
            

            //Puts the narrowed down search results into the records object as we will now refine the results if we have multiple search terms
            records = tempStorage;            

            //Returns the final results
            return records;
        }

        //This function deals with ordering the search results and determining what should be returned
        private DataTable ReturnResults(DataTable searchResults)
        {
            //Loops to see if we have search results at the end of the processing
            if (searchResults.Rows.Count > 0)
            {
                string orderby = "";

                //Checks if the flag is active and if makes the first ordering by the weight of the records
                if (settings.orderByWeightFirst)
                {
                    orderby += string.Format(",{0} DESC", "dotnetsearch_search_weight");
                }

                //Checks if we have more orderbys
                if (settings.orderBy != null && settings.orderBy.Count > 0)
                {
                    //Orders the orderby output via its priority so we do it in the order it was specified
                    var orderPri = settings.orderBy.OrderBy(l => l.Value.priorityLevel);

                    foreach (var item in orderPri)
                    {
                        orderby += string.Format(",{0} {1}", item.Key, ((item.Value.isDescending) ? "DESC" : "ASC"));
                    }
                }

                //If we actually have any compiled order by then remove the first comma and use it for the view
                if (!string.IsNullOrWhiteSpace(orderby))
                {
                    //Removes the first comma
                    orderby = orderby.Remove(0,1);

                    //Orders by our compiled orderby statement
                    searchResults.DefaultView.Sort = orderby;
                }

                //Returns the number of search records specified
                //Note: if the value is 0 or below it pulls everything!
                if (settings.maxReturn <= 0)
                {
                    return searchResults.Select().CopyToDataTable();
                }
                else
                {
                    return searchResults.Select().Take(settings.maxReturn).CopyToDataTable();
                }
            }

            //Returns the same datatable passed as its empty and we need the columns to be the same
            return searchResults;
        }
    }
}
