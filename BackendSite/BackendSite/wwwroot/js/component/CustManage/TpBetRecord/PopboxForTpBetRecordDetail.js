import React from "react";
import ReactJsonView from "react-json-view";
import ReactJsonViewer, { propTypes } from "react-json-viewer";

const DateDetailModal = ({
  show,
  showRawDetail,
  GameTypeInfo,
  tpBetRecordDatas,
  handleSwitch,
  handleClose,
}) => {
  let mainDetail = null;
  let rawDetail = null;
  let urlShow = false;
  let url = null;

  if (show) {
    let strMessage = null;

    switch (tpBetRecordDatas.errorCode) {
      case 0: //已經接廠商，取得資料成功(Object呈現)
        {
          if (tpBetRecordDatas.data != null) {
            let dataKeys = Object.keys(tpBetRecordDatas.data);
            if (dataKeys.find((element) => element == "detail")) {
              rawDetail = JSON.parse(tpBetRecordDatas.data.detail);
              mainDetail = Object.assign({}, tpBetRecordDatas.data);

              delete mainDetail.detail;
            } else {
              strMessage =
                GameTypeInfo.GameTypeName + " Data(detail of result) not find.";
              rawDetail = { message: strMessage };
              mainDetail = { message: strMessage };
            }
          } else {
            strMessage =
              GameTypeInfo.GameTypeName + " Data not find. Data is null.";
            rawDetail = { message: strMessage };
            mainDetail = { message: strMessage };
          }
        }
        break;
      case 3000: //已經接廠商，取得資料找不到
        {
          strMessage = GameTypeInfo.GameTypeName + " Data not find.";
          rawDetail = { message: strMessage };
          mainDetail = { message: strMessage };
        }
        break;
      case 3001: //尚未接廠商，取得資料找不到
        {
          strMessage = GameTypeInfo.GameTypeName + " Data not connected yet.";
          rawDetail = { message: strMessage };
          mainDetail = { message: strMessage };
        }
        break;
      case 3002: //已經接廠商，取得資料成功(Url呈現)
        {
          console.log("tpBetRecordDatas", tpBetRecordDatas);
          if (tpBetRecordDatas.data != null) {
            let dataKeys = Object.keys(tpBetRecordDatas.data);
            console.log("dataKeys", dataKeys);
            if (dataKeys.find((element) => element == "detailUrl")) {
              rawDetail = Object.assign({}, tpBetRecordDatas.data);
              delete rawDetail.detail;

              urlShow = true;
              url = tpBetRecordDatas.data.detailUrl;
              console.log("url", url);
            } else {
              strMessage =
                GameTypeInfo.GameTypeName + " Data(detail of result) not find.";
              rawDetail = { message: strMessage };
              mainDetail = { message: strMessage };
            }
          } else {
            strMessage =
              GameTypeInfo.GameTypeName + " Data not find. Data is null.";
            rawDetail = { message: strMessage };
            mainDetail = { message: strMessage };
          }
        }
        break;
      default:
        //已經接廠商，取得資料exception & 未知錯誤
        {
          strMessage =
            GameTypeInfo.GameTypeName +
            " Data error happened. Please contant admin.";
          rawDetail = { message: strMessage };
          mainDetail = { message: strMessage };
        }
        break;
    }
  }

  const showHideStyle = show
    ? { display: "block", background: "rgba(0, 0, 0, 0.6)" }
    : {};
  if (urlShow) {
    return (
      //Url顯示
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
              className="btn btn-light"
              style={
                !showRawDetail
                  ? {
                      background: "black",
                      color: "white",
                      position: "absolute",
                      left: "31px",
                    }
                  : { position: "absolute", left: "31px" }
              }
              onClick={() => handleSwitch(false)}
              disabled={!showRawDetail}
            >
              Main Detail
            </button>
            <button
              type="button"
              className="btn btn-light"
              style={
                showRawDetail
                  ? {
                      background: "black",
                      color: "white",
                      position: "absolute",
                      left: "141px",
                    }
                  : { position: "absolute", left: "141px" }
              }
              onClick={() => handleSwitch(true)}
              disabled={showRawDetail}
            >
              Raw Detail
            </button>

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
            {showRawDetail ? (
              <ReactJsonViewer json={rawDetail} />
            ) : (
              <table className="table table-detail">
                <tbody>
                  <tr>
                    <th scope="row">Detail</th>
                    <td width="100%">
                      <a href={url} target="_blank">
                        Click To Show Detail
                      </a>
                    </td>
                  </tr>
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>
    );
  } else {
    return (
      //一般顯示
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
              className="btn btn-light"
              style={
                !showRawDetail
                  ? {
                      background: "black",
                      color: "white",
                      position: "absolute",
                      left: "31px",
                    }
                  : { position: "absolute", left: "31px" }
              }
              onClick={() => handleSwitch(false)}
              disabled={!showRawDetail}
            >
              Main Detail
            </button>
            <button
              type="button"
              className="btn btn-light"
              style={
                showRawDetail
                  ? {
                      background: "black",
                      color: "white",
                      position: "absolute",
                      left: "141px",
                    }
                  : { position: "absolute", left: "141px" }
              }
              onClick={() => handleSwitch(true)}
              disabled={showRawDetail}
            >
              Raw Detail
            </button>

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
            {showRawDetail ? (
              <ReactJsonView
                src={rawDetail}
                theme="rjv-default"
                iconStyle="square"
                displayDataTypes={false}
              />
            ) : (
              <ReactJsonViewer json={mainDetail} />
            )}
          </div>
        </div>
      </div>
    );
  }
};

export default DateDetailModal;
