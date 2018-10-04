DELIMITER $$

-- CALL yari_test_4(3,'rey',UTC_TIMESTAMP());
-- Definition for procedure get_clients
--
DROP PROCEDURE IF EXISTS yari_test_4$$
CREATE PROCEDURE yari_test_4(age int, name varchar(50), birthday datetime)
BEGIN

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        RESIGNAL;
    END;

    SELECT age, name, birthday;

END
$$
DELIMITER;
