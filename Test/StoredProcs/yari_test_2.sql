DELIMITER $$

-- CALL yari_test_2();
-- Definition for procedure get_clients
--
DROP PROCEDURE IF EXISTS yari_test_2$$
CREATE PROCEDURE yari_test_2()
BEGIN

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        RESIGNAL;
    END;

    SELECT 1 AS 'id', 'name1' AS 'name'
    UNION ALL 
    SELECT 2 AS 'id', 'name2' AS 'name';

END
$$
DELIMITER;

