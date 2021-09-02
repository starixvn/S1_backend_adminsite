import React from "react";
import ReactJsonView from "react-json-view";
import ReactJsonViewer, { propTypes } from "react-json-viewer";

const DateDetailModal = ({
  show,
  showRawDetail,
  tpBetRecordDatas,
  handleSwitch,
  handleClose,
}) => {
  let bodyRows = [];
  let mainDetail = null;
  let rawDetail = null;
  if (show) {
    rawDetail = JSON.parse(tpBetRecordDatas.data.detail);
    mainDetail = Object.assign({}, tpBetRecordDatas.data);
    console.log(mainDetail);
    delete mainDetail.detail;
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
          <table className="table table-striped">
            <tbody>{bodyRows}</tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default DateDetailModal;
