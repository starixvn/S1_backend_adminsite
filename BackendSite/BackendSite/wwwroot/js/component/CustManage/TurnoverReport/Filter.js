import DateTimePicker from "react-datetime";

const defDateType = {
  0: "",
  1: "Today",
  2: "Yesterday",
  3: "This Week",
  4: "Last Week",
  5: "This Month",
  6: "Last Month",
};

const Filter = (props) => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment.utc(new Date()).add(8,'hour').add(-1, "days").format("l LTS"),
    EndTime: moment.utc(new Date()).add(8,'hour').format("l LTS"),
    DateQuickPicker: "0"
  });

  const EditDateRangeFiled = (v, col) => {
    setDateRange({
      ...DateRangeDatas,
      [col]: v,
    });
  };
  const EditDate = (v, col) =>{
    let _startDate = "";
    let _endDate = "";

    switch (v) {
      case "1":
          _startDate = moment().startOf("day");
          _endDate = moment().endOf("day");
        break;
      case "2":
        _startDate = moment().subtract(1, "days");
        _endDate = moment().subtract(1, "days");

        break;
      case "3":
        _startDate = moment().startOf("week");
        _endDate = moment().endOf("week");

        break;
      case "4":
        _startDate = moment().subtract(1, "week").startOf("week");
        _endDate = moment().subtract(1, "week").endOf("week");

        break;
      case "5":
        _startDate = moment().startOf("month");
        _endDate = moment().endOf("month");

        break;
      case "6":
        _startDate = moment().subtract(1, "month").startOf("month");
        _endDate = moment().subtract(1, "month").endOf("month");

        break;
      default:
        break;
    }

    if (v !== "0") {
      setDateRange({
        ...DateRangeDatas,
        ["StartTime"]: _startDate,
        ["EndTime"]: _endDate,
        ["DateQuickPicker"]:v
      });
    }else{
      setDateRange({
        ...DateRangeDatas,
        ["DateQuickPicker"]:v
      });
    }
  }

  const [GameListDatas, setGameList] = React.useState([]);
  React.useEffect(() => {
    axios
      .post("/api/CustManage/GetAllGameTypeList", {})
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setGameList(result.data);
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  }, []);

  const [SelectedGames, SetGames] = React.useState({
    selectAll: false,
    selected: [],
  });

  const OnSelectGame = (v) => {
    let selectAll = SelectedGames.selectAll;
    let tmpG = [...SelectedGames.selected];
    if (v === -1) {
      selectAll = !selectAll;
      if (selectAll) {
        tmpG = GameListDatas.map((g) => {
          return g.type;
        });
      } else {
        tmpG = [];
      }
    } else {
      let getIdx = tmpG.indexOf(v);
      if (getIdx === -1) {
        tmpG.push(v);
      } else {
        tmpG.splice(getIdx, 1);
      }

      selectAll = tmpG.length === GameListDatas.length;
    }

    SetGames({
      selectAll: selectAll,
      selected: [...tmpG],
    });
  };

  const ClickSubmit = () => {
    props.ClickSubmit({
      ...DateRangeDatas,
      GameTypes: SelectedGames.selected,
    });
  };

  return (
    <form className="filter" style={{ display: "block" }}>
      <div className="row-flex">
        <DateTimePickerElem
          columnTitle="StartTime"
          dateTimePickerProps={{
            value:DateRangeDatas.StartTime,
            defaultValue: DateRangeDatas.StartTime,
            onBlur: (e) => {
              try {
                EditDateRangeFiled(moment(e).format("l LTS"), "StartTime");
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="EndTime"
          dateTimePickerProps={{
            value:DateRangeDatas.EndTime,
            defaultValue: DateRangeDatas.EndTime,
            onBlur: (e) => {
              try {
                EditDateRangeFiled(moment(e).format("l LTS"), "EndTime");
              } catch (ex) {}
            },
          }}
        />
        <DropDownBox
          columnTitle="Date Quick Picker"
          options={Object.keys(defDateType).map((key) => {
            return { value: key, displayText: defDateType[key] };
          })}
          hasAll={false}
          value={DateRangeDatas.DateQuickPicker}
          dataColumn="DateQuickPicker"
          onChange={(e)=>EditDate(e,"DateQuickPicker")}
        />
      </div>
      <div className="row-flex">
        <GameList
          SelectedGames={SelectedGames}
          OnSelectGame={OnSelectGame}
          GameListDatas={GameListDatas}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="button"
            onClick={ClickSubmit}
          >
            Submit
          </button>
        </div>
        <div>(Adjusted to GMT+8)</div>
      </div>
    </form>
  );
};

const GameList = (props) => {
  return (
    <>
      <div className="checkBoxTitle">GAMES</div>
      <CheckBox
        Key={`Game-all`}
        checked={props.SelectedGames.selectAll}
        onChange={props.OnSelectGame.bind(null, -1)}
        Title="All"
        Value={-1}
      />
      {props.GameListDatas.map((g) => {
        return (
          <CheckBox
            key={`cb-game-${g.type}`}
            Key={`Game-${g.type}`}
            checked={props.SelectedGames.selected.includes(g.type)}
            onChange={props.OnSelectGame.bind(null, g.type)}
            Title={g.typeName}
            Value={g.type}
          />
        );
      })}
    </>
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

const CheckBox = (props) => {
  const OnChange = (e) => {
    //e.preventDefault();
    props.onChange();
  };
  return (
    <label className="checkbox checkbox-checked">
      <input
        type="checkbox"
        style={{ width: "0" }}
        value={props.Value}
        checked={props.checked}
        onChange={OnChange}
      />
      <i className="icon icon-checkbox"></i>
      <span className="text">{props.Title}</span>
    </label>
  );
};

const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <div className="field">
        <select value={props.value} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};

export default Filter;
