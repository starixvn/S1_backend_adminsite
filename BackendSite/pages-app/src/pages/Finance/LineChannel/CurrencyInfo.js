import React, { useState, useEffect } from "react";
import { LineChannelInfo } from "./LineChannelInfo";
import { PopBoxContentAddPoint } from "./PopboxContentAddPoint";
import { api } from "../../../module/WebAPI";

if (process.env.NODE_ENV !== "production") {
  window.i18n = {
    lbl_Currency1: "SGD",
    lbl_Currency2: "MYR",
    lbl_Currency3: "USD",
    lbl_Currency4: "THB",
    lbl_Currency6: "EURO",
    lbl_Currency9: "AUD",
    lbl_Currency12: "GBP",
    lbl_Currency13: "CNY",
    lbl_Currency15: "IDR",
    lbl_Currency20: "UUS",
    lbl_Currency32: "JPY",
    lbl_Currency40: "WON",
    lbl_Currency41: "CHF",
    lbl_Currency45: "WON",
    lbl_Currency51: "VND",
    lbl_Currency61: "INR",
    lbl_Save: "Save",
    lbl_Success: "Success",
    lbl_Action: "Action",
    lbl_SelectSite: "Select site",
    lbl_Point: "Point",
    lbl_DepositLeast: "Deposit least",
    lbl_BetLeast: "Bet least",
    lbl_AddPointLevel: "Add point level",
    lbl_AddNewPointLevel: "Add new point level",
    lbl_Delete: "Delete",
    lbl_AreYouSure: "Are you sure?",
    lbl_OK: "OK",
    lbl_Cancel: "Cancel",
    lbl_Error: "Error",
  };
}

export const CurrencyInfo = (props) => {
  let CurrencyList = props.optSite.siteCurrencyList.map((value) => {
    return (
      <div key={`CurrencyInfo-${value}`} className="panel mt-20">
        <a
          className="panel-title"
          data-toggle="collapse"
          href={`#collapseExample-${value}`}
          role="button"
          aria-expanded="true"
          aria-controls="collapseExample"
        >
          {window.i18n["lbl_Currency" + value]}
        </a>
        <Result siteId={props.optSite.siteId} currId={value} />
      </div>
    );
  });
  return CurrencyList;
};

const Result = (props) => {
  const [stateEdit, setEdit] = useState({});
  const [editValue, setEditValue] = useState(null);
  const [isOpen, setOpen] = useState(false);
  const [lineChannelList, setLineChannelList] = useState(null);
  const [statusValue, setstatusValue] = useState({});

  const handleOpen = (_isOpen) => {
    setOpen(_isOpen);
  };

  const handleAddLineChannelList = (_newList) => {
    let mounted = true;
    api
      .post("/api/LineChannel/GetList", {
        siteId: props.siteId,
        pageNumber: 1,
        pageSize: 100,
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          if (mounted) {
            setLineChannelList(
              response.data === null ? [] : response.data.data
            );
          }
        } else {
        }
      })
      .catch(function (error) {
        alert(error);
      });
    setOpen(false);
  };

  const handleEdit = (idx) => {
    let isEdit = stateEdit[idx] === true ? false : true;
    if (editValue !== null) {
      setEditValue(null);
    }
    setEdit({
      [idx]: isEdit,
    });
  };

  const handleSetLineChannelList = (idx, status, Link, dstatus) => {
    let tmp = [...lineChannelList];
    if (status === "Delete") {
      tmp.splice(idx, 1);
      setLineChannelList(tmp);
    } else if (status === "Update") {
      tmp[idx].link = Link;
      tmp[idx].status = dstatus;

      setLineChannelList(tmp);
    }
  };

  const handleDelete = (idx) => {
    if (window.confirm("Are you sure ?") === true) {
      api
        .post("/api/LineChannel/Delete", { Id: lineChannelList[idx].id })
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLineChannelList(idx, "Delete");
            alert("delete successful.");
          } else {
            alert(response.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const handleUpdate = (idx) => {
    if (editValue !== null) {
      let Link =
        editValue.link === undefined
          ? lineChannelList[idx].link
          : editValue.link;
      let Status =
        editValue.status === undefined
          ? lineChannelList[idx].status
          : editValue.status
          ? 1
          : 0;
      api
        .post("/api/LineChannel/Set", {
          isAdd: false,
          id: lineChannelList[idx].id,
          siteId: props.siteId,
          currencyId: props.currId,
          link: Link,
          status: parseInt(Status),
        })
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLineChannelList(idx, "Update", Link, Status);
            alert("update successful.");
            handleEdit(idx);
          } else {
            alert(response.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };
  const handleStatusChange = (idx, _newData) => {
    if (statusValue !== null) {
      setstatusValue(null);
    }
    setstatusValue({ [idx]: _newData });
    let tmp = [...lineChannelList],
      status = _newData ? 1 : 0;
    tmp[idx].status = status;
    setLineChannelList(tmp);
    let param = {
      Id: lineChannelList[idx].id,
      link: lineChannelList[idx].link,
      status: status,
    };

    api
      .post("/api/LineChannel/Set", {
        isAdd: false,
        id: lineChannelList[idx].id,
        siteId: props.siteId,
        currencyId: props.currId,
        link: lineChannelList[idx].link,
        status: parseInt(status),
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          alert("update successful.");
        } else {
          alert(response.data.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const ChangeData = (idx, _locate, _newData) => {
    setEditValue({
      ...editValue,
      idx: idx,
      [_locate]: _newData,
    });
  };

  useEffect(() => {
    let mounted = true;
    api
      .post("/api/LineChannel/GetList", {
        siteId: props.siteId,
        pageNumber: 1,
        pageSize: 100,
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          if (mounted) {
            setLineChannelList(
              response.data === null ? [] : response.data.data
            );
          }
        } else {
          alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
    return () => (mounted = false);
  }, []);

  if (lineChannelList === null) {
    return <></>;
  } else {
    return (
      <div className="panel-body">
        <div className="collapse show" id={`collapseExample-${props.currId}`}>
          <div className="table">
            <div className="table-options">
              <button
                className="btn btn-add"
                onClick={handleOpen.bind(null, true)}
              >
                Add
              </button>
            </div>
            <LineChannelInfo
              lineChannelList={lineChannelList}
              CurrencyId={props.currId}
              handleEdit={handleEdit}
              handleUpdate={handleUpdate}
              handleDelete={handleDelete}
              EditValue={editValue}
              StateEdit={stateEdit}
              StatusValue={statusValue}
              ChangeData={ChangeData}
              handleStatusChange={handleStatusChange}
            />
          </div>
          {isOpen ? (
            <PopBoxContentAddPoint
              ClickCancel={handleOpen.bind(null, false)}
              lineChannelList={lineChannelList}
              SiteId={props.siteId}
              CurrencyId={props.currId}
              ClickSave={handleAddLineChannelList}
            />
          ) : null}
        </div>
      </div>
    );
  }
};
