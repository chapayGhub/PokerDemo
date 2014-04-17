using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Starcounter;
using Vendigo;

namespace PlayersDemoGui {
    public class ResponseHandler : IResponseHandler {
        // Number of correct responses.
        volatile Int32 numGoodResps_ = 0;

        // Help string builder.
        StringBuilder strBuilder_ = new StringBuilder();

        // Reference to Gui.
        InterfaceObject gui_ = null;

        // Reference to requests provider.
        RequestProvider reqProvider_ = null;

        // Gets number of good responses.
        public Int32 NumGoodResponses {
            get { return numGoodResps_; }
        }

        // Constructor.
        public ResponseHandler(RequestProvider rp, InterfaceObject gui) {
            gui_ = gui;
            reqProvider_ = rp;
        }

        // Resetting the requests creator.
        public void Reset() {
            numGoodResps_ = 0;
            reqProvider_.Reset();
        }

        // Batched response processor.
        void IResponseHandler.ProcessResponseBatch(Response[] responseBatch, int count) {
            
            for (int i = 0; i < count; i++) {
                var response = responseBatch[i];

                // Checking for correct status code.
                if (response.StatusCode == 200 || response.StatusCode == 201) {
                    // Increasing number of good responses.
                    numGoodResps_++;

                    // Diagnostics.
                    if ((numGoodResps_ % 100000) == 0)
                        Console.WriteLine("   {0} good responses of {1}", numGoodResps_, reqProvider_.RequestsCreator.TotalPlannedRequestsNum);

                    // Processing response body if it exists.

                    IntPtr ptr;
                    int size;

                    response.GetBodyRaw(out ptr, out size);

                    if (size > 0) {
                        // Checking the Gui paused flag.
                        if (!gui_.IsPaused) {
                            // Checking if we can update.
                            if (gui_.ResponseUpdateFlag) {
                                PlayerAndAccounts p = new PlayerAndAccounts();

                                p.PopulateFromJson(ptr, size);

                                // Converting header to string.
                                String header = response.Headers;

                                // Deserializing header and body to string.
                                strBuilder_.Append(header);
                                strBuilder_.Append(p.ToJson());
                                String decodedResponse = strBuilder_.ToString();
                                strBuilder_.Clear();

                                // Setting the decoded string.
                                gui_.DecodedResponseString = decodedResponse; //.Insert(decodedResponse.IndexOf("L:") + 15, decodedBodyLen.ToString());

                                // Creating encoded response string.
                                //String encodedBody = ASCIIEncoding.ASCII.GetString(responseBatch.Body);
                                String encodedResponse = header + p.ToJson();

                                // Setting the encoded string.
                                gui_.EncodedResponseString = encodedResponse;

                                // Setting the correct string.
                                if (gui_.IsEncoded)
                                    gui_.Output = gui_.EncodedResponseString;
                                else
                                    gui_.Output = gui_.DecodedResponseString;

                                // Disabling response update.
                                gui_.ResponseUpdateFlag = false;
                            }
                        }
                    } else {
                        // Showing just the header since body is not present.
                        // Checking the paused flag.
                        if (!gui_.IsPaused) {
                            // Checking if we can update.
                            if (gui_.ResponseUpdateFlag) {
                                // Deserializing header to string.
                                String decodedResponse = response.Headers;

                                // Setting the decoded string.
                                gui_.DecodedResponseString = decodedResponse;

                                // Setting the encoded string.
                                gui_.EncodedResponseString = decodedResponse;

                                // Setting the correct string.
                                if (gui_.IsEncoded)
                                    gui_.Output = gui_.EncodedResponseString;
                                else
                                    gui_.Output = gui_.DecodedResponseString;

                                // Disabling response update.
                                gui_.ResponseUpdateFlag = false;
                            }
                        }
                    }
                } else {
                    MessageBox.Show(response.Body, "Error occurred!");
                    throw new Exception("Bad response received when it should not!");
                }
            }

            // Checking if we are done with processing.
            if (numGoodResps_ >= reqProvider_.RequestsCreator.TotalPlannedRequestsNum) {
                // Printing the score.
                Console.WriteLine("Done!");
            }
        }
    }
}
