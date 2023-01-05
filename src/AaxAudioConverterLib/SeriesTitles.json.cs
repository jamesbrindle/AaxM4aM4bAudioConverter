namespace audiamus.aaxconv.lib.json
{


    public class AppSeriesTitles
    {
        public string[] response_groups { get; set; }
        public SimilarProducts[] similar_products { get; set; }
    }

    public class SimilarProducts : Product
    {
    }

}
