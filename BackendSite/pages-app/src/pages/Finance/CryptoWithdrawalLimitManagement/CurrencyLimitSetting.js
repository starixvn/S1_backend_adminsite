import React, { useState, useEffect } from "react";
import { LevelInfo } from "./LevelInfo";
import { AddLevel } from "./AddLevel";
import WebAPI from "../../../module/WebAPI";

const CurrencyLimitSetting = (props) => {
  const [limitList, setLimitList] = useState([]);
  const [getListTrigger, setGetListTrigger] = useState(0);

  useEffect(() => {
    if (!props.siteCurrencyList
      || !props.networkType
      || !props.cryptoCurrencyId) {
      setLimitList([]);
    }
    else {
      let siteIds = [];
      siteIds.push(props.siteId);
      let param = {
        Url: "apiV2/WithdrawalLimitCrypto/GetList",
        Data: {
          siteIds: siteIds,
          currencyIds: props.siteCurrencyList,
          networkType: props.networkType,
          cryptoCurrency: props.cryptoCurrencyId,
          pageNumber: 1,
          pageSize: 1000
        }
      };

      WebAPI.PostData(param)
        .then((res) => {
          console.log(res.data);
          if (res.data.errorCode === 0) {
            setLimitList(res.data === null ? [] : res.data.data);
          } else {
            alert(res.data.message || "failed to get list data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  }, [props, getListTrigger]);

  if (!props.siteCurrencyList
    || !props.networkType
    || !props.cryptoCurrencyId) {
    return <></>;
  }

  const doGetList = () => setGetListTrigger(getListTrigger + 1);

  let CurrencyList = props.siteCurrencyList.map((value) => {
    let datas = limitList.filter(x => x.currencyId === value);
    return (
      <div
        key={`Site-${props.siteId}-CurrencyInfo-${value}-CryptoCurrency-${props.cryptoCurrencyId}-Network-${props.network}`}
        className="panel mt-20"
      >
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
        <Result siteId={props.siteId}
          currencyId={value}
          cryptoCurrencyId={props.cryptoCurrencyId}
          networkType={props.networkType}
          list={datas}
          doGetList={doGetList} />
      </div>
    );
  });
  return CurrencyList;
};


const Result = (props) => {
  const [stateEdit, setEdit] = useState({});
  const [editValue, setEditValue] = useState(null);
  const [isOpen, setOpen] = useState(false);
  let levelList = props.list;

  const handleOpen = (_isOpen) => {
    setOpen(_isOpen);
  };

  const handleAddLevelList = (_newList) => {
    levelList = [...levelList, _newList];
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

  const handleSetLevelList = (idx, status, withdrawalMin, withdrawalMax, times) => {
    let tmp = [...levelList];
    if (status === "Delete") {
      tmp.splice(idx, 1);
      levelList = tmp;
    } else if (status === "Update") {
      tmp[idx].withdrawalMin = withdrawalMin;
      tmp[idx].withdrawalMax = withdrawalMax;
      tmp[idx].times = times;
      levelList = tmp;
    }
  };

  const handleDelete = (idx) => {
    if (window.confirm("您真的確定要刪除嗎？\n請確認！") === true) {
      let param = {
        Url: "apiV2/WithdrawalLimitCrypto/Delete",
        Data: {
          siteId: levelList[idx].siteId,
          currencyId: levelList[idx].currencyId,
          vipLevel: levelList[idx].vipLevel,
          cryptoCurrency: levelList[idx].cryptoCurrencyId,
          networkType: levelList[idx].networkType
        }
      };
      WebAPI.PostData(param)
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLevelList(idx, "Delete");
            props.doGetList();
            alert(response.data.message || "Update successful.");
          } else {
            alert(response.data.message || "Process failed.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const handleUpdate = (idx) => {
    if (editValue !== null) {
      let withdrawalMin =
        typeof editValue.withdrawalMin === "undefined"
          ? levelList[idx].withdrawalMin
          : editValue.withdrawalMin;
      let withdrawalMax =
        typeof editValue.withdrawalMax === "undefined"
          ? levelList[idx].withdrawalMax
          : editValue.withdrawalMax;
      let times =
        typeof editValue.times === "undefined"
          ? levelList[idx].times
          : editValue.times;
      let param = {
        Url: "apiV2/WithdrawalLimitCrypto/Update",
        Data: {
          siteId: levelList[idx].siteId,
          currencyId: levelList[idx].currencyId,
          vipLevel: levelList[idx].vipLevel,
          cryptoCurrency: levelList[idx].cryptoCurrencyId,
          networkType: levelList[idx].networkType,
          withdrawalMin: parseInt(withdrawalMin),
          withdrawalMax: parseInt(withdrawalMax),
          times: times * 1
        }
      };
      WebAPI.PostData(param)
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLevelList(idx, "Update", withdrawalMin, withdrawalMax, times);
            alert(response.data.message || "Update successful.");
            handleEdit(idx);
          } else {
            alert(response.data.message || "Process failed.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const ChangeData = (idx, _locate, _newData) => {
    setEditValue({
      ...editValue,
      idx: idx,
      [_locate]: _newData,
    });
  };

  if (levelList === null) {
    return <></>;
  } else {
    return (
      <div className="panel-body">
        <div className="collapse show" id={`collapseExample-${props.currencyId}`}>
          <div className="table">
            <div className="table-options">
              <button
                className="btn btn-add"
                onClick={handleOpen.bind(null, true)} >
                Add point level
              </button>
            </div>
            <LevelInfo
              levelList={levelList}
              handleEdit={handleEdit}
              handleUpdate={handleUpdate}
              handleDelete={handleDelete}
              editValue={stateEdit}
              changeData={ChangeData}
            />
          </div>
          {isOpen ? (
            <AddLevel
              clickCancel={handleOpen.bind(null, false)}
              levelList={levelList}
              siteId={props.siteId}
              currencyId={props.currencyId}
              clickSave={handleAddLevelList}
              networkType={props.networkType}
              cryptoCurrencyId={props.cryptoCurrencyId}
              doGetList={props.doGetList} />
          ) : null}
        </div>
      </div>
    );
  }
};

export default CurrencyLimitSetting;