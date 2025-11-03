players-- 1. 데이터 베이스 생성
CREATE DATABASE `gameDB` /*!40100 COLLATE 'utf8mb4_0900_ai_ci' */;

-- 2. 테이블 생성
CREATE TABLE `players` (
	`player_ID` INT NOT NULL AUTO_INCREMENT,
	`username` VARCHAR(50) NULL DEFAULT '0',
	`email` VARCHAR(50) NULL DEFAULT '0',
	`password_hash` VARCHAR(255) NULL DEFAULT '0',
	`create_at` TIMESTAMP NULL NULL DEFAULT,
	`last_login` TIMESTAMP NULL NULL DEFAULT,
	PRIMARY KEY (`player_ID`),
	UNIQUE INDEX `username` (`username`),
	UNIQUE INDEX `email` (`email`)
)

-- 3. 플레이어 데이터 삽입
INSERT INTO players(username, email, password_hash) VALUES
('hero223', 'hero1@gmail.com', 'hased_password2'),
('hero323', 'hero2@gmail.com', 'hased_password3'),
('hero423', 'hero3@gmail.com', 'hased_password4'),
('hero523', 'hero4@gmail.com', 'hased_password5')

-- 4. 플레이어 데이터 조회
SELECT * FROM players
SELECT username, last_login FROM players

-- 5. 특정 플레이어 정보 업데이트
UPDATE players SET last_login = CURRENT_TIMESTAMP WHERE username = 'hero223'

-- 6. 조건에 맞는 플레이어 검색
SELECT username, email FROM players WHERE username LIKE '%hero%'

-- 7. 플레이어 삭제
DELETE FROM players WHERE username = 'hero123'

-- 8. 플레이어 테이블에 새 열 추가
ALTER TABLE players ADD COLUMN `level` INT DEFAULT 1

-- 9. 모든 플레이어 level을 1 증가
UPDATE players SET `level` = `level` + 1

-- 10. 가장 level이 높은 플레이어 가져오기
SELECT username, `level` FROM players ORDER BY `level` DESC LIMIT 1
