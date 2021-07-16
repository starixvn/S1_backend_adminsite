import React, { useState } from "react";
import * as Loading from "./Loading";
import { api } from "../../../module/WebAPI";

const PictureModal = ({ data, handleClose, show }) => {
  const [hasPictureStatus, sethasPictureStatus] = useState(null);
  const [stateData, setStateData] = useState(data);
  const showHideStyle = show
    ? { display: "block", background: "rgba(0, 0, 0, 0.6)" }
    : {};

  const Upload = (event) => {
    Loading.OnLoading();
    let imgindex = 0;
    if (event.clipboardData.files.length) {
      for (var i = 0; i < event.clipboardData.items.length; i++) {
        if (event.clipboardData.items[i].type == "image/png") {
          imgindex = i;
        }
      }
    }
    var blob = event.clipboardData.items[imgindex].getAsFile();
    var reader = new FileReader();
    const scope = this;
    reader.onload = function (event) {
      api
        .post("/api/WithdrawalBankAccounting/UploadAccountingPicture", {
          Picture: event.target.result,
          Id: data.id,
          TransId: data.transId,
          Seq: data.seq,
        })
        .then(
          async function (response) {
            Loading.UnLoading();
            if (response.data.errorCode == 0) {
              data.uploadPicTimes = data.uploadPicTimes + 1;
              //setStateData(data);
              sethasPictureStatus(true);
              scope.onRefresh();
            } else {
            }
          }.bind(this)
        )
        .catch(function (error) {}.bind(this));
    }; // data url!

    reader.readAsDataURL(blob);
  };

  const Delete = () => {
    let Data = {
      TransId: data.transId,
      Seq: data.seq,
      UploadPicTimes: data.uploadPicTimes,
    };
    if (window.confirm(`Are you sure to delete picture ?`)) {
      Loading.OnLoading();
      api
        .post("/api/WithdrawalBankAccounting/DeleteAccountingPicture", Data)
        .then(function (res) {
          Loading.UnLoading();
          if (res.data.errorCode === 0) {
            sethasPictureStatus(false);
          } else {
            alert(res.data.message ?? "check picture error.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
    Loading.UnLoading();
  };

  const ShowPic = () => {
    if (data != null) {
      if (hasPictureStatus == null) {
        if (data.hasPicture) {
          return true;
        } else {
          return false;
        }
      } else {
        if (hasPictureStatus) {
          return true;
        } else {
          return false;
        }
      }
    } else {
      return false;
    }
  };

  let picrows = null;
  if (data != null) {
    if (ShowPic()) {
      picrows = (
        <div>
          <img
            className="img-slip"
            src={
              "/api/WithdrawalBankAccounting/GetAccountingPicture?transId=" +
              data.transId +
              "&seq=" +
              data.seq +
              "&uploadPicTimes=" +
              data.uploadPicTimes
            }
          />
          <div>
            <button
              type="button"
              className="btn btn-back pull-left"
              onClick={Delete.bind(this)}
            >
              Delete
            </button>
          </div>
        </div>
      );
    } else {
      picrows = (
        <div onPaste={Upload.bind(this)} style={{ height: "100%" }}>
          <div
            style={{
              marginTop: 5,
              padding: 10,
              fontStyle: "italic",
              color: "red",
              border: "1px solid red",
              height: "80%",
            }}
          >
            Paste Area
          </div>
        </div>
      );
    }
  }

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
        {picrows}
      </div>
    </div>
  );
};

export default PictureModal;
