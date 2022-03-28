import React, { useState } from "react";
import DateTimePicker from "react-datetime";

const defShowStatusType = {
  1: "No login",
  2: "Login",
  3: "Both",
};
const initShowStatusType = [{ value: 3, displayText: defShowStatusType[3] }];
export const defActionModeType = {
  1: "No Action",
  2: "Go to Game",
  3: "Go to Bonus",
  4: "Open Window",
  5: "Redirect Page",
  6: "Redirect pages of this site",
};

export const initActionModeType = [
  { value: 2, displayText: defActionModeType[2] },
];

const Panel = (props) => {
  let TitleDetail =
    "https://docs.google.com/spreadsheets/d/1f2ONaTvR5Bj3nBuWK4W7fkuZO24pYVh41IThrrieqB8/edit#gid=0";
  let sportsLeaguesList = props.sportsLeaguesList.map((s) => {
    return {
      value: s.id,
      displayText: s.name,
    };
  });
  let sportsTeamsList = props.sportsTeamsList.map((s) => {
    return {
      value: s.id,
      displayText: s.name,
    };
  });
  return (
    <div className="container-fluid" id="Panel">
      <div className="filter noBGcolor"></div>
      <div className="panel level-1 mt-20">
        <div
          className="panel-title no-arrow"
          style={{ justifyContent: "space-between" }}
        >
          <span style={{ color: "white" }}>HotEvent Info</span>
        </div>
        <div id="searchForm">
          <div className="filter">
            <div className="row-flex">
              <InputBox
                Title="Banner Name"
                OnChange={props.EditBannerData}
                dataColumn="bannerName"
                value={props.bannerData.bannerName}
                Disabled={props.pageStatus == 2}
              />
            </div>
          </div>
          <div className="filter">
            <RadioBox
              Title="Action Mode"
              Options={initActionModeType}
              OnChange={props.EditBannerData}
              dataColumn="actionMode"
              Selected={props.bannerData.actionMode}
            />
            <RadioBox
              Title="Show Status"
              Options={initShowStatusType}
              OnChange={props.EditBannerData}
              dataColumn="showStatus"
              Selected={props.bannerData.showStatus}
            />
            <div className="row-flex">
              <DateTimePickerElem
                columnTitle="Start Time"
                dateTimePickerProps={{
                  value: props.bannerData.startTime,
                  defaultValue: props.bannerData.startTime,
                  onChange: (e) => {
                    try {
                      props.EditBannerData(e, "startTime");
                    } catch (ex) {}
                  },
                }}
              />
              <DateTimePickerElem
                columnTitle="End Time"
                dateTimePickerProps={{
                  value: props.bannerData.endTime,
                  defaultValue: props.bannerData.endTime,
                  onChange: (e) => {
                    try {
                      props.EditBannerData(e, "endTime");
                    } catch (ex) {}
                  },
                }}
              />
            </div>
            <div className="row-flex">
              <InputBox
                Title="Match Id"
                OnChange={props.EditBannerData}
                dataColumn="matchId"
                value={props.bannerData.matchId}
              />
              <InputBox
                Title="Tp Id"
                TitleDetail={TitleDetail}
                OnChange={props.EditBannerData}
                dataColumn="tpId"
                value={props.bannerData.tpId}
              />
              <InputBox
                Title="Game Id"
                OnChange={props.EditBannerData}
                dataColumn="gameId"
                value={props.bannerData.gameId}
              />
            </div>
            <div className="row-flex">
              <DateTimePickerElem
                columnTitle="Sport Battle Date"
                dateTimePickerProps={{
                  value: props.bannerData.sportBattledate,
                  defaultValue: props.bannerData.sportBattledate,
                  onChange: (e) => {
                    try {
                      props.EditBannerData(e, "sportBattledate");
                    } catch (ex) {}
                  },
                }}
              />
            </div>
            <div className="row-flex">
              <SelectBox
                Title="Sport Leagues"
                Options={sportsLeaguesList}
                HandleChange={props.changeSportLeaguesId}
                dataColumn="sportLeaguesId"
                SelectedValue={props.bannerData.sportLeaguesId}
              />
            </div>
            <div className="row-flex">
              <SelectBox
                Title="Home Sport Teams"
                Options={sportsTeamsList}
                HandleChange={props.EditBannerData}
                dataColumn="home"
                SelectedValue={props.bannerData.home}
              />
              <SelectBox
                Title="Away Sport Teams"
                Options={sportsTeamsList}
                HandleChange={props.EditBannerData}
                dataColumn="away"
                SelectedValue={props.bannerData.away}
              />
            </div>
          </div>
        </div>
        <div className="filter float-btn-group">
          <button
            className="btn btn-back"
            onClick={props.ClickBack}
          >
            Back
          </button>
          <button
            className="btn btn-add"
            onClick={props.ClickSubmit}
          >
            Submit
          </button>
        </div>
      </div>
    </div>
  );
};
const InputBox = (props) => {
  const OnChange = (e) => {
    let value = e.target.value;
    let checked =
      value === "" || props.regex === undefined
        ? true
        : value.match(props.regex);
    if (checked) {
      props.OnChange(value, props.dataColumn);
    }
  };
  return (
    <div className={props.ClassName ? props.ClassName : "input-item"}>
      {props.HideTitle === true ? null : (
        <label className="label">
          {props.Title}{" "}
          {props.TitleDetail ? (
            <a href={props.TitleDetail} target="_blank">
              {" "}
              (Detail){" "}
            </a>
          ) : null}
        </label>
      )}
      <div
        className={`field${props.iptSubClass ? " " + props.iptSubClass : ""}`}
      >
        {props.SubTitle ? (
          <span className="bule-bold">{props.SubTitle}</span>
        ) : null}
        <input
          type={props.type ? props.type : "text"}
          value={
            props.value === null || props.value === undefined ? "" : props.value
          }
          onChange={OnChange}
          disabled={props.Disabled === true}
          className={props.inputClass ? props.inputClass : ""}
        />
      </div>
    </div>
  );
};

const RadioBox = ({
  Title,
  Options,
  Selected,
  OnChange,
  dataColumn,
  mainKey = null,
}) => {
  let opt = Options.map((o, oIdx) => {
    let key = `${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`;
    return (
      <RadioBtn
        key={key}
        Id={key}
        DisplayText={o.displayText}
        Value={`${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        dataColumn={dataColumn}
        Name={`${Title}${mainKey === null ? "" : `-${mainKey}`}`}
        OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
        Checked={Selected === o.value}
      />
    );
  });
  return (
    <>
      <div className="row-flex">
        <div className="input-item">
          <label className="label">{Title}</label>
        </div>
      </div>
      <div className="row-flex">{opt}</div>
    </>
  );
};

const RadioBtn = ({
  Id,
  DisplayText,
  Value,
  Name,
  OnChange,
  Checked,
  Disabled,
}) => {
  return (
    <div className="custom-control custom-radio custom-control-inline">
      <input
        type="radio"
        id={Id}
        name={Name}
        className="custom-control-input revert-style"
        value={Value}
        onChange={OnChange}
        checked={Checked}
        disabled={true}
      />
      <label className="custom-control-label" htmlFor={Id}>
        {DisplayText}
      </label>
    </div>
  );
};
const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};
const SelectBox = ({
  Options,
  HandleChange,
  SelectedValue,
  dataColumn,
  Disabled,
  Title,
  iptSubClass,
}) => {
  const OnChange = (e) => {
    HandleChange(e.target.value, dataColumn);
  };
  return (
    <div className="input-item">
      {Title ? <label className="label">{Title}</label> : null}
      <div className={`field${iptSubClass ? " " + iptSubClass : ""}`}>
        <select
          className="field"
          value={SelectedValue}
          onChange={OnChange}
          disabled={Disabled}
        >
          {Options &&
            Options.map((o) => {
              return (
                <option key={o.value} value={o.value}>
                  {o.displayText}
                </option>
              );
            })}
        </select>
      </div>
    </div>
  );
};
export default Panel;
