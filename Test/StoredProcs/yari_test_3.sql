DELIMITER $$

-- CALL yari_test_3();
-- Definition for procedure get_clients
--
DROP PROCEDURE IF EXISTS yari_test_3$$
CREATE PROCEDURE yari_test_3()
BEGIN

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        RESIGNAL;
    END;

    SELECT 1 AS 'id';

END
$$
DELIMITER;

