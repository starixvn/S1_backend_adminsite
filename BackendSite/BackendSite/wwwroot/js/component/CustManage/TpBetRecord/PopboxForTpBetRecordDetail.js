import React from "react";

const checkDataType = (data) => {
  //判斷是否為array
  if (Array.isArray(data)) {
    return "array";
  }
  //判斷是否為object
  if (typeof data == "object") {
    return "object";
  }
  return "text";
};

const DateDetailModal = ({ show, tpBetRecordDatas, handleClose }) => {
  let bodyRows = [];

  if (show) {
    let detail = JSON.parse(tpBetRecordDatas.data.detail); //將資料轉json
    let detailDataType = checkDataType(detail); //判斷detail型態

    //第1層資料處理(用detail資料型態判斷)
    switch (detailDataType) {
      case "array":
        //detail=0.array.
        break;
      case "object":
        //detail=0.object.
        let detailKeys = Object.keys(detail);
        detailKeys.forEach((firstObjectKey, firstObjectIdx) => {
          let firstObjectProperty = checkDataType(detail[firstObjectKey]);

          console.log(
            "detail is object",
            1,
            firstObjectKey,
            firstObjectProperty
          ); //for debug

          //第2層資料處理(用1層資料型態判斷)
          switch (firstObjectProperty) {
            case "array":
              //detail=0.object >> 1.array.
              detail[firstObjectKey].forEach(
                (secondArrayItem, firstArrayIdx) => {
                  //Array需要Display目前輸出是[firstArrayIdx]筆
                  bodyRows.push(
                    <tr key={`result-body-0-object-1-array-${firstArrayIdx}`}>
                      <td>{`${firstObjectKey}[${firstArrayIdx}] ▼`}</td>
                    </tr>
                  );
                  let secondArrayProperty = checkDataType(secondArrayItem);

                  console.log(
                    "detail is object",
                    1,
                    firstObjectKey,
                    firstObjectProperty,
                    2,
                    " >> array",
                    firstArrayIdx,
                    secondArrayProperty
                  ); //for debug

                  //第3層資料處理(用2層資料型態判斷)
                  switch (secondArrayProperty) {
                    case "array":
                      //detail=0.object >> 1.array >> 2.array
                      break;
                    case "object":
                      //detail=0.object >> 1.array >> 2.object
                      let secondItemKeys = Object.keys(secondArrayItem); //取出第2層object屬性
                      secondItemKeys.forEach(
                        (secondItemKey, secondArrayItemIdx) => {
                          let thirdObjectProperty = checkDataType(
                            secondArrayItem[secondItemKey]
                          );
                          console.log(
                            "detail is object",
                            1,
                            firstObjectKey,
                            firstObjectProperty,
                            2,
                            " >> array",
                            firstArrayIdx,
                            secondArrayProperty,
                            3,
                            " >> object",
                            secondItemKey,
                            thirdObjectProperty
                          ); //for debug

                          switch (thirdObjectProperty) {
                            case "array":
                              //detail=0.object >> 1.array >> 2.object >> 3.array
                              secondArrayItem[secondItemKey].forEach(
                                (thirdArrayItem, secondArrayIdx) => {
                                  bodyRows.push(
                                    <tr
                                      key={`result-body-0-object-1-array-2-object-3-array-${secondArrayIdx}`}
                                    >
                                      <td>{`${firstObjectKey}[${firstArrayIdx}] ➤ ${secondItemKey}[${secondArrayIdx}] ▼`}</td>
                                    </tr>
                                  );
                                  let fourthArrayProperty =
                                    checkDataType(thirdArrayItem);
                                  console.log(
                                    "detail is object",
                                    1,
                                    firstObjectKey,
                                    firstObjectProperty,
                                    2,
                                    " >> array",
                                    firstArrayIdx,
                                    secondArrayProperty,
                                    3,
                                    " >> object",
                                    secondItemKey,
                                    thirdObjectProperty,
                                    4,
                                    ">> array",
                                    secondArrayIdx,
                                    fourthArrayProperty
                                  ); //for debug

                                  switch (fourthArrayProperty) {
                                    case "array":
                                      //detail=0.object >> 1.array >> 2.object >> 3.array >> 4.array
                                      break;
                                    case "object":
                                      //detail=0.object >> 1.array >> 2.object >> 3.array >> 4.object
                                      let fourthObjectKeys =
                                        Object.keys(thirdArrayItem); //取出第2層object屬性
                                      fourthObjectKeys.forEach(
                                        (fourthObjectKey, fourthObjectIdx) => {
                                          let fifthObjectProperty =
                                            checkDataType(
                                              thirdArrayItem[fourthObjectKey]
                                            );
                                          console.log(
                                            "detail is object",
                                            1,
                                            firstObjectKey,
                                            firstObjectProperty,
                                            2,
                                            " >> array",
                                            firstArrayIdx,
                                            secondArrayProperty,
                                            3,
                                            " >> object",
                                            secondItemKey,
                                            thirdObjectProperty,
                                            4,
                                            ">> array",
                                            secondArrayIdx,
                                            fourthArrayProperty,
                                            5,
                                            ">> object",
                                            fourthObjectKey,
                                            fifthObjectProperty
                                          ); //for debug
                                          switch (fifthObjectProperty) {
                                            case "array":
                                              break;
                                            case "object":
                                              break;
                                            default:
                                              //detail=0.object >> 1.array >> 2.object >> 3.array >> 4.object >> 5.text
                                              bodyRows.push(
                                                <tr
                                                  key={`result-body-0-object-1-array-2-object-3-array-4-object-5-text-${fourthObjectIdx}`}
                                                >
                                                  <td>{`${firstObjectKey}[${firstArrayIdx}] ➤ ${secondItemKey}[${secondArrayIdx}] ➤ ${fourthObjectKey}`}</td>
                                                  <td>
                                                    {
                                                      thirdArrayItem[
                                                        fourthObjectKey
                                                      ]
                                                    }
                                                  </td>
                                                </tr>
                                              );
                                              break;
                                          }
                                        }
                                      );
                                      break;
                                    default:
                                      //detail=0.object >> 1.array >> 2.object >> 3.array >> 4.text
                                      bodyRows.push(
                                        <tr
                                          key={`result-body-0-object-1-array-2-object-3-array-4-text-${secondArrayIdx}`}
                                        >
                                          <td>{`${firstObjectKey}[${firstArrayIdx}] ➤ ${secondItemKey}[${secondArrayIdx}]`}</td>
                                          <td>{thirdArrayItem}</td>
                                        </tr>
                                      );
                                      break;
                                  }
                                }
                              );
                              break;
                            case "object":
                              //detail=0.object >> 1.array >> 2.object >> 3.object
                              break;
                            default:
                              //detail=0.object >> 1.array >> 2.object >> 3.text
                              bodyRows.push(
                                <tr
                                  key={`result-body-0-object-1-array-2-object-3-text-${secondArrayItemIdx}`}
                                >
                                  <td>{`${firstObjectKey}[${firstArrayIdx}] ➤ ${secondItemKey}`}</td>
                                  <td>{secondArrayItem[secondItemKey]}</td>
                                </tr>
                              );
                              break;
                          }
                        }
                      );
                      break;
                    default:
                      //detail=0.object >> 1.array >> 2.text
                      bodyRows.push(
                        <tr
                          key={`result-body-0-object-1-array-2-text-${firstArrayIdx}`}
                        >
                          <td>{`${firstObjectKey}[${firstArrayIdx}]`}</td>
                          <td>{secondArrayItem}</td>
                        </tr>
                      );
                      break;
                  }
                }
              );
              break;
            case "object":
              //detail=0.object >> 1.object.
              break;
            default:
              //detail=0.object >> 1.text.
              console.log("detail >> " + detail[firstObjectKey] + " is text"); //for debug
              bodyRows.push(
                <tr key={`result-body-0-object-1-text-${firstObjectIdx}`}>
                  <td>{firstObjectKey}</td>
                  <td>{detail[firstObjectKey]}</td>
                </tr>
              );
              break;
          }
        });
        break;
      default:
        //detail=0.text.
        console.log("detail is text"); //for debug
        bodyRows.push(
          <tr key={`result-body-0-text`}>
            <td>Detail</td>
            <td>{detail}</td>
          </tr>
        );
        break;
    }
  }

  const showHideStyle = show
    ? { display: "block", background: "rgba(0, 0, 0, 0.6)" }
    : {};
  return (
    <div className="modal" style={showHideStyle}>
      <div
        style={{
          position: "absolute",
          width: "90%",
          height: "90%",
          background: "#ffffff",
          left: "5%",
          top: "5%",
          padding: "10px",
        }}
      >
        <div
          style={{
            background: "#E9E9E9",
            borderBottom: "0",
            padding: "8px",
            display: "flex",
            border: "1px solid #d5d5d5",
            height: "49px",
          }}
        >
          <button
            type="button"
            className="btn btn-approved"
            style={{ position: "absolute", right: "31px" }}
            onClick={handleClose}
          >
            Close
          </button>
        </div>
        <div style={{ overflow: "scroll", height: "85%" }}>
          <table className="table table-striped">
            <tbody>{bodyRows}</tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default DateDetailModal;
