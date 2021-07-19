import DateTimePicker from "react-datetime";

const Filter = (props) => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment(new Date()).add(-1, "days").format("l LTS"),
    EndTime: moment(new Date()).format("l LTS"),
  });

  const EditDateRangeFiled = (v, col) => {
    setDateRange({
      ...DateRangeDatas,
      [col]: v,
    });
  };

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
            defaultValue: DateRangeDatas.EndTime,
            onBlur: (e) => {
              try {
                EditDateRangeFiled(moment(e).format("l LTS"), "EndTime");
              } catch (ex) {}
            },
          }}
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

export default Filter;
