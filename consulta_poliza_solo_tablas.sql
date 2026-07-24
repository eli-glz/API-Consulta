-- Consulta equivalente a la respuesta actual de la API, sin usar packages.
-- Cambiar solamente el valor de NUMERO_POLIZA en PARAMETROS.
-- Consulta de solo lectura: no ejecuta DML ni DDL.

WITH PARAMETROS AS (
    SELECT 203561 AS NUMERO_POLIZA
      FROM DUAL
),
BASE_POLICY AS (
    SELECT PO.NPOLICY,
           PO.SCERTYPE,
           PO.NBRANCH,
           CE.NPRODUCT,
           CE.SCLIENT,
           CE.DSTARTDATE,
           CE.DEXPIRDAT,
           PO.STYP_MODULE,
           ROW_NUMBER() OVER (
               ORDER BY CASE WHEN CE.NCERTIF = 0 THEN 0 ELSE 1 END,
                        CE.NCERTIF
           ) AS RN
      FROM INSUDBGEN.POLICY PO
      INNER JOIN INSUDBGEN.CERTIFICAT CE
              ON CE.NBRANCH = PO.NBRANCH
             AND CE.NPOLICY = PO.NPOLICY
             AND CE.SCERTYPE = PO.SCERTYPE
             AND CE.SCLIENT = PO.SCLIENT
      CROSS JOIN PARAMETROS X
     WHERE PO.NPOLICY = X.NUMERO_POLIZA
),
P AS (
    SELECT *
      FROM BASE_POLICY
     WHERE RN = 1
)
SELECT P.NPOLICY AS NUMERO_POLIZA,
       CAST(NULL AS VARCHAR2(1)) AS ESTADO,
       (SELECT CASE
                   WHEN COUNT(*) = 1
                   THEN MAX(TRIM(REPLACE(CL.SCLIENAME, CHR(160), ' ')))
                   ELSE ' '
               END
          FROM INSUDB.CLIENT CL
         WHERE CL.SCLIENT = P.SCLIENT) AS ASEGURADO,
       (SELECT CASE
                   WHEN COUNT(*) = 1 THEN MAX(TRIM(PM.SDESCRIPT))
               END
          FROM INSUDBGEN.PRODMASTER PM
         WHERE PM.NBRANCH = P.NBRANCH
           AND PM.NPRODUCT = P.NPRODUCT) AS PRODUCTO,
       P.DSTARTDATE AS VIGENCIA_DESDE,
       P.DEXPIRDAT AS VIGENCIA_HASTA,
       (SELECT CASE
                   WHEN COUNT(*) = 1 THEN MAX(TRIM(T.SDESCRIPT))
               END
          FROM INSUDBGEN.TABLE10 T
         WHERE T.NBRANCH = P.NBRANCH) AS RAMO,
       TRIM(P.SCLIENT) AS NUMERO_CLIENTE,
       CASE NVL(P.STYP_MODULE, '*')
           WHEN '3' THEN COALESCE(
               (SELECT CASE
                           WHEN COUNT(DISTINCT C.NCOVER) = 1 THEN MIN(C.NCOVER)
                       END
                  FROM INSUDBGEN.COVER_CO_G C
                  INNER JOIN INSUDBGEN.LIFE_COVER L
                          ON L.NBRANCH = C.NBRANCH
                         AND L.NPRODUCT = C.NPRODUCT
                         AND L.NMODULEC = C.NMODULEC
                         AND L.NCOVER = C.NCOVER
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND C.NROLE = 2
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE)
                   AND L.SCOVERUSE = '1'
                   AND L.DEFFECDATE <= P.DSTARTDATE
                   AND (L.DNULLDATE IS NULL OR L.DNULLDATE > P.DSTARTDATE)),
               (SELECT MIN(C.NCOVER)
                  FROM INSUDBGEN.COVER_CO_G C
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE))
           )
           WHEN '2' THEN COALESCE(
               (SELECT CASE
                           WHEN COUNT(DISTINCT C.NCOVER) = 1 THEN MIN(C.NCOVER)
                       END
                  FROM INSUDBGEN.COVER_CO_P C
                  INNER JOIN INSUDBGEN.LIFE_COVER L
                          ON L.NBRANCH = C.NBRANCH
                         AND L.NPRODUCT = C.NPRODUCT
                         AND L.NMODULEC = C.NMODULEC
                         AND L.NCOVER = C.NCOVER
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND C.NROLE = 2
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE)
                   AND L.SCOVERUSE = '1'
                   AND L.DEFFECDATE <= P.DSTARTDATE
                   AND (L.DNULLDATE IS NULL OR L.DNULLDATE > P.DSTARTDATE)),
               (SELECT MIN(C.NCOVER)
                  FROM INSUDBGEN.COVER_CO_P C
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE))
           )
           ELSE COALESCE(
               (SELECT CASE
                           WHEN COUNT(DISTINCT C.NCOVER) = 1 THEN MIN(C.NCOVER)
                       END
                  FROM INSUDBGEN.COVER C
                  INNER JOIN INSUDBGEN.LIFE_COVER L
                          ON L.NBRANCH = C.NBRANCH
                         AND L.NPRODUCT = C.NPRODUCT
                         AND L.NMODULEC = C.NMODULEC
                         AND L.NCOVER = C.NCOVER
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND C.NROLE = 2
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE)
                   AND L.SCOVERUSE = '1'
                   AND L.DEFFECDATE <= P.DSTARTDATE
                   AND (L.DNULLDATE IS NULL OR L.DNULLDATE > P.DSTARTDATE)),
               (SELECT MIN(C.NCOVER)
                  FROM INSUDBGEN.COVER C
                 WHERE C.SCERTYPE = P.SCERTYPE
                   AND C.NBRANCH = P.NBRANCH
                   AND C.NPRODUCT = P.NPRODUCT
                   AND C.NPOLICY = P.NPOLICY
                   AND C.DEFFECDATE <= P.DSTARTDATE
                   AND (C.DNULLDATE IS NULL OR C.DNULLDATE > P.DSTARTDATE))
           )
       END AS COBERTURA_PRINCIPAL
  FROM P;
