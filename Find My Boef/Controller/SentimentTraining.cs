using Find_My_Boef.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Linq;

namespace Find_My_Boef.Controller
{
    public static class SentimentTraining
    {
        /// <summary>
        /// Training of the sentiment analysis
        /// </summary>
        /// <param name="url"></param>
        public static void Train(string url)
        {
            MLContext mlContext = new(seed: 1);
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(url, separatorChar: ',', hasHeader: true);

            // Split the data set for training
            DataOperationsCatalog.TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            IDataView testData = trainTestSplit.TestSet;

            // Turn text input into a feature
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

            // Use the label in the dataset to create a classification trainer
            var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Train the sentiment analysis
            ITransformer trainedModel = trainingPipeline.Fit(dataView);

            // Use the testdata to verify the model
            IDataView predictions = trainedModel.Transform(testData);
            CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

            // Save to trained model to a file
            mlContext.Model.Save(trainedModel, dataView.Schema, "..\\..\\..\\..\\Find my boef\\controller\\Training\\TrainData.ZIP");
        }
    }
}
