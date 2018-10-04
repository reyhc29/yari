DELIMITER $$

-- CALL yari_test_1('{"age":1,"name":"yari","jobs":["developer","support","mathematichian"]}');
-- Definition for procedure get_clients
--
DROP PROCEDURE IF EXISTS yari_test_1$$
CREATE PROCEDURE yari_test_1(params json)
BEGIN

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        RESIGNAL;
    END;
   
    DROP TEMPORARY TABLE IF EXISTS jobs;
    CREATE TEMPORARY TABLE jobs
    SELECT *
    FROM JSON_TABLE(params->"$.jobs", "$[*]" COLUMNS(  
        jobs varchar(100) PATH "$"
    )) AS tt;


    SELECT 
        CAST(params->"$.age" AS signed) AS 'age',
        params->"$.name" AS 'name',
        params->"$.job" AS 'job'
        ;

    SELECT * 
    FROM JSON_TABLE(params->"$.jobs", "$[*]" COLUMNS(  
        jobs varchar(100) PATH "$"
    )) AS tt;

    SELECT * FROM jobs;

END
$$
DELIMITER;

